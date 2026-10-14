using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Phase 7 规范清理回归测试
/// 覆盖：
/// - MonoBehaviour 类名必须与文件名一致（Unity 脚本资产绑定要求，防止改名回潮）；
/// - Scripts 下目录名不含空格（防止带空格目录回潮，路径引用易错）；
/// - PlaySFX/StopSFX 调用点不再出现裸整数下标（必须走 SfxIds 常量表）。
/// </summary>
public class NamingTests
{
    private static string ScriptsDir => Path.Combine(Application.dataPath, "Scripts");

    // ==================== 类名/文件名一致性 ====================
    [Test]
    public void MonoBehaviour_ClassName_Matches_FileName()
    {
        // NTFS 大小写不敏感，不能用 File.Exists 判定，需枚举真实文件名做 Ordinal 比较
        var fileNames = new HashSet<string>(
            Directory.GetFiles(ScriptsDir, "*.cs", SearchOption.AllDirectories)
                     .Select(Path.GetFileNameWithoutExtension),
            StringComparer.Ordinal);

        var offenders = new List<string>();
        foreach (Type type in typeof(Player).Assembly.GetTypes())
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(type))
                continue;
            if (type.IsNested) // 嵌套类型不要求独立文件
                continue;
            if (type.IsGenericType) // 泛型基类（如 Singleton<T>）运行时名带反引号，文件名单复数约定不适用
                continue;
            if (!fileNames.Contains(type.Name))
                offenders.Add(type.FullName);
        }

        Assert.IsEmpty(offenders,
            "以下 MonoBehaviour 类名找不到同名 .cs 文件（GUID 绑定依赖该约定）：\n" + string.Join("\n", offenders));
    }

    // ==================== 目录名无空格 ====================
    [Test]
    public void Script_Directories_Have_No_Spaces()
    {
        var offenders = Directory.GetDirectories(ScriptsDir, "*", SearchOption.AllDirectories)
                                 .Where(d => Path.GetFileName(d).Contains(" "))
                                 .Select(Path.GetFileName)
                                 .ToList();

        Assert.IsEmpty(offenders,
            "Scripts 下存在带空格的目录（路径字符串引用易出错）：" + string.Join(", ", offenders));
    }

    // ==================== SFX 调用点无魔法数字 ====================
    [Test]
    public void SFX_CallSites_Use_Named_Constants()
    {
        // 匹配 PlaySFX(数字 / StopSFX(数字 形式的裸下标调用（变量/常量传参不命中）
        var bareIndex = new Regex(@"(?:PlaySFX|StopSFX)\(\s*-?\d");
        var offenders = new List<string>();

        foreach (string file in Directory.GetFiles(ScriptsDir, "*.cs", SearchOption.AllDirectories))
        {
            var matches = bareIndex.Matches(File.ReadAllText(file));
            foreach (Match m in matches)
                offenders.Add($"{Path.GetFileName(file)}: {m.Value}");
        }

        Assert.IsEmpty(offenders,
            "以下调用点仍使用裸整数音效下标（应改用 SfxIds 常量）：\n" + string.Join("\n", offenders));
    }
}
