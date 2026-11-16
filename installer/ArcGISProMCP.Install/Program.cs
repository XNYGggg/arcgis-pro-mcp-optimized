using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

const string Repo = "XNYGggg/arcgis-pro-mcp-optimized";
const string DefaultTag = "v2.0.1";

string tag = DefaultTag;
string? zipOverride = null;
bool dryRun = false;

try { Console.OutputEncoding = Encoding.UTF8; } catch { /* 重定向输出时可能不支持 */ }

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--tag":
            if (i + 1 >= args.Length) return UsageError("--tag 需要一个参数");
            tag = args[++i];
            break;
        case "--zip":
            if (i + 1 >= args.Length) return UsageError("--zip 需要一个参数");
            zipOverride = args[++i];
            break;
        case "--dry-run":
            dryRun = true;
            break;
        case "-h":
        case "--help":
            PrintUsage();
            return 0;
        default:
            return UsageError($"未知参数: {args[i]}");
    }
}

Console.WriteLine($"ArcGIS Pro MCP 安装器 · {tag}");

string workRoot = Path.Combine(Path.GetTempPath(), "arcgis-pro-mcp-install", tag.TrimStart('v', 'V'));
string zipPath = zipOverride is null ? Path.Combine(workRoot, "package.zip") : Path.GetFullPath(zipOverride);
string extractDir = Path.Combine(workRoot, "extracted");

if (zipOverride is null)
{
    string version = tag.TrimStart('v', 'V');
    string url = $"https://github.com/{Repo}/releases/download/{tag}/ArcGIS-Pro-MCP-{version}.zip";
    Console.WriteLine($"[1/4] 下载: {url}");
    Directory.CreateDirectory(workRoot);
    try
    {
        using var http = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("ArcGISProMCP.Install/2.0.1");
        using var resp = http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
        if (!resp.IsSuccessStatusCode)
        {
            Console.Error.WriteLine($"下载失败: HTTP {(int)resp.StatusCode}。请确认发布标签 {tag} 与附件名一致。");
            return 3;
        }
        long total = resp.Content.Headers.ContentLength ?? -1;
        using var src = resp.Content.ReadAsStream();
        using var dst = File.Create(zipPath);
        byte[] buffer = new byte[81920];
        long done = 0;
        int n;
        while ((n = src.Read(buffer, 0, buffer.Length)) > 0)
        {
            dst.Write(buffer, 0, n);
            done += n;
            if (total > 0) Console.Write($"\r      {done * 100.0 / total,6:F1}%   {done / 1048576.0,7:F1} / {total / 1048576.0:F1} MB");
        }
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"下载失败: {ex.Message}");
        return 3;
    }
}
else
{
    if (!File.Exists(zipPath)) { Console.Error.WriteLine($"找不到 zip 文件: {zipPath}"); return 3; }
    Console.WriteLine($"[1/4] 使用本地 zip: {zipPath}");
}

Console.WriteLine("[2/4] 解压…");
if (Directory.Exists(extractDir)) Directory.Delete(extractDir, true);
ZipFile.ExtractToDirectory(zipPath, extractDir, overwriteFiles: true);

string? packageRoot = Directory.GetDirectories(extractDir)
    .FirstOrDefault(d => File.Exists(Path.Combine(d, "SHA256SUMS.txt")));
if (packageRoot is null)
{
    Console.Error.WriteLine("包结构异常: 未找到含 SHA256SUMS.txt 的根目录。");
    return 1;
}

Console.WriteLine("[3/4] 校验 SHA256SUMS.txt…");
string sumsPath = Path.Combine(packageRoot, "SHA256SUMS.txt");
string rootFull = Path.GetFullPath(packageRoot);
int ok = 0, bad = 0;
foreach (string rawLine in File.ReadAllLines(sumsPath, Encoding.UTF8))
{
    string line = rawLine.Trim().Trim('\r');
    if (line.Length == 0) continue;
    int split = line.IndexOf(" *", StringComparison.Ordinal);
    if (split <= 0)
    {
        Console.Error.WriteLine($"  无法解析清单行: {line}");
        bad++;
        continue;
    }
    string expected = line[..split].Trim().ToLowerInvariant();
    string rel = line[(split + 2)..].Trim();
    if (rel.StartsWith("./", StringComparison.Ordinal)) rel = rel[2..];
    string full = Path.GetFullPath(Path.Combine(rootFull, rel));
    if (!full.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase) || !File.Exists(full))
    {
        Console.Error.WriteLine($"  缺失: {rel}");
        bad++;
        continue;
    }
    using var fs = File.OpenRead(full);
    string actual = Convert.ToHexString(SHA256.HashData(fs)).ToLowerInvariant();
    if (actual == expected) ok++;
    else
    {
        Console.Error.WriteLine($"  不匹配: {rel}");
        bad++;
    }
}
Console.WriteLine($"      {ok} 个文件通过, {bad} 个失败。");
if (bad > 0)
{
    Console.Error.WriteLine("校验未通过，已中止安装。");
    return 1;
}

string? installCmd = Directory.GetFiles(extractDir, "00-install.cmd", SearchOption.AllDirectories)
    .FirstOrDefault(p => p.Replace('\\', '/').Contains("/install/"));
if (installCmd is null)
{
    Console.Error.WriteLine("包内未找到 install/00-install.cmd。");
    return 1;
}

if (dryRun)
{
    Console.WriteLine($"[4/4] dry-run: 将执行 \"{installCmd}\"，工作目录 {packageRoot}。未启动安装。");
    return 0;
}

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    Console.Error.WriteLine("安装步骤仅支持 Windows（ArcGIS Pro 平台）。可用 --dry-run 在其它系统上仅做下载与校验。");
    return 4;
}

Console.WriteLine("[4/4] 启动安装程序（如需提权会弹出 UAC 确认）…");
var psi = new ProcessStartInfo
{
    FileName = "cmd.exe",
    Arguments = $"/c \"\"{installCmd}\"\"",
    WorkingDirectory = packageRoot,
    UseShellExecute = false,
};
using var proc = Process.Start(psi);
if (proc is null)
{
    Console.Error.WriteLine("无法启动安装进程。");
    return 4;
}
proc.WaitForExit();
if (proc.ExitCode != 0)
{
    Console.Error.WriteLine($"安装脚本退出码 {proc.ExitCode}。可到 {extractDir} 手动排查。");
    return 4;
}
Console.WriteLine("完成。解压目录保留在: " + extractDir);
return 0;

static int UsageError(string message)
{
    Console.Error.WriteLine(message);
    PrintUsage();
    return 2;
}

static void PrintUsage()
{
    Console.WriteLine("""
        用法: arcgis-pro-mcp-install [选项]

        从 GitHub Releases 下载 ArcGIS Pro MCP 离线包，逐文件校验 SHA256SUMS.txt 后
        运行 install/00-install.cmd 完成安装。

        选项:
          --tag <tag>   指定发布标签（默认 v2.0.1）
          --zip <path>  改用本地 zip（跳过下载，仍做完整校验）
          --dry-run     只下载与校验，不启动安装
          -h, --help    显示本帮助
        """);
}
