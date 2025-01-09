namespace TableConverter.DataGeneration;

public static class LanguageMapping
{
    public static readonly IReadOnlyDictionary<string, string> CyrillicMapping = new Dictionary<string, string>
    {
        { "А", "A" }, { "а", "a" }, { "Б", "B" }, { "б", "b" }, { "В", "V" }, { "в", "v" },
        { "Г", "G" }, { "г", "g" }, { "Д", "D" }, { "д", "d" }, { "ъе", "ye" }, { "Ъе", "Ye" },
        { "ъЕ", "yE" }, { "ЪЕ", "YE" }, { "Е", "E" }, { "е", "e" }, { "Ё", "Yo" }, { "ё", "yo" },
        { "Ж", "Zh" }, { "ж", "zh" }, { "З", "Z" }, { "з", "z" }, { "И", "I" }, { "и", "i" },
        { "ый", "iy" }, { "Ый", "Iy" }, { "ЫЙ", "IY" }, { "ыЙ", "iY" }, { "Й", "Y" }, { "й", "y" },
        { "К", "K" }, { "к", "k" }, { "Л", "L" }, { "л", "l" }, { "М", "M" }, { "м", "m" },
        { "Н", "N" }, { "н", "n" }, { "О", "O" }, { "о", "o" }, { "П", "P" }, { "п", "p" },
        { "Р", "R" }, { "р", "r" }, { "С", "S" }, { "с", "s" }, { "Т", "T" }, { "т", "t" },
        { "У", "U" }, { "у", "u" }, { "Ф", "F" }, { "ф", "f" }, { "Х", "Kh" }, { "х", "kh" },
        { "Ц", "Ts" }, { "ц", "ts" }, { "Ч", "Ch" }, { "ч", "ch" }, { "Ш", "Sh" }, { "ш", "sh" },
        { "Щ", "Sch" }, { "щ", "sch" }, { "Ъ", "" }, { "ъ", "" }, { "Ы", "Y" }, { "ы", "y" },
        { "Ь", "" }, { "ь", "" }, { "Э", "E" }, { "э", "e" }, { "Ю", "Yu" }, { "ю", "yu" },
        { "Я", "Ya" }, { "я", "ya" }
    };

    public static readonly IReadOnlyDictionary<string, string> GreekMapping = new Dictionary<string, string>
    {
        { "α", "a" }, { "β", "v" }, { "γ", "g" }, { "δ", "d" }, { "ε", "e" }, { "ζ", "z" },
        { "η", "i" }, { "θ", "th" }, { "ι", "i" }, { "κ", "k" }, { "λ", "l" }, { "μ", "m" },
        { "ν", "n" }, { "ξ", "ks" }, { "ο", "o" }, { "π", "p" }, { "ρ", "r" }, { "σ", "s" },
        { "τ", "t" }, { "υ", "y" }, { "φ", "f" }, { "χ", "x" }, { "ψ", "ps" }, { "ω", "o" },
        { "ά", "a" }, { "έ", "e" }, { "ί", "i" }, { "ό", "o" }, { "ύ", "y" }, { "ή", "i" },
        { "ώ", "o" }, { "ς", "s" }, { "ϊ", "i" }, { "ΰ", "y" }, { "ϋ", "y" }, { "ΐ", "i" },
        { "Α", "A" }, { "Β", "B" }, { "Γ", "G" }, { "Δ", "D" }, { "Ε", "E" }, { "Ζ", "Z" },
        { "Η", "I" }, { "Θ", "TH" }, { "Ι", "I" }, { "Κ", "K" }, { "Λ", "L" }, { "Μ", "M" },
        { "Ν", "N" }, { "Ξ", "KS" }, { "Ο", "O" }, { "Π", "P" }, { "Ρ", "R" }, { "Σ", "S" },
        { "Τ", "T" }, { "Υ", "Y" }, { "Φ", "F" }, { "Χ", "X" }, { "Ψ", "PS" }, { "Ω", "O" },
        { "Ά", "A" }, { "Έ", "E" }, { "Ί", "I" }, { "Ό", "O" }, { "Ύ", "Y" }, { "Ή", "I" },
        { "Ώ", "O" }, { "Ϊ", "I" }, { "Ϋ", "Y" }
    };

    public static readonly IReadOnlyDictionary<string, string> ArabicMapping = new Dictionary<string, string>
    {
        { "ء", "e" }, { "آ", "a" }, { "أ", "a" }, { "ؤ", "w" }, { "إ", "i" }, { "ئ", "y" },
        { "ا", "a" }, { "ب", "b" }, { "ة", "t" }, { "ت", "t" }, { "ث", "th" }, { "ج", "j" },
        { "ح", "h" }, { "خ", "kh" }, { "د", "d" }, { "ذ", "dh" }, { "ر", "r" }, { "ز", "z" },
        { "س", "s" }, { "ش", "sh" }, { "ص", "s" }, { "ض", "d" }, { "ط", "t" }, { "ظ", "z" },
        { "ع", "e" }, { "غ", "gh" }, { "ـ", "_" }, { "ف", "f" }, { "ق", "q" }, { "ك", "k" },
        { "ل", "l" }, { "م", "m" }, { "ن", "n" }, { "ه", "h" }, { "و", "w" }, { "ى", "a" },
        { "ي", "y" }, { "َ‎", "a" }, { "ُ", "u" }, { "ِ‎", "i" }
    };

    public static readonly IReadOnlyDictionary<string, string> ArmenianMapping = new Dictionary<string, string>
    {
        { "ա", "a" }, { "Ա", "A" }, { "բ", "b" }, { "Բ", "B" }, { "գ", "g" }, { "Գ", "G" },
        { "դ", "d" }, { "Դ", "D" }, { "ե", "ye" }, { "Ե", "Ye" }, { "զ", "z" }, { "Զ", "Z" },
        { "է", "e" }, { "Է", "E" }, { "ը", "y" }, { "Ը", "Y" }, { "թ", "t" }, { "Թ", "T" },
        { "ժ", "zh" }, { "Ժ", "Zh" }, { "ի", "i" }, { "Ի", "I" }, { "լ", "l" }, { "Լ", "L" },
        { "խ", "kh" }, { "Խ", "Kh" }, { "ծ", "ts" }, { "Ծ", "Ts" }, { "կ", "k" }, { "Կ", "K" },
        { "հ", "h" }, { "Հ", "H" }, { "ձ", "dz" }, { "Ձ", "Dz" }, { "ղ", "gh" }, { "Ղ", "Gh" },
        { "ճ", "tch" }, { "Ճ", "Tch" }, { "մ", "m" }, { "Մ", "M" }, { "յ", "y" }, { "Յ", "Y" },
        { "ն", "n" }, { "Ն", "N" }, { "շ", "sh" }, { "Շ", "Sh" }, { "ո", "vo" }, { "Ո", "Vo" },
        { "չ", "ch" }, { "Չ", "Ch" }, { "պ", "p" }, { "Պ", "P" }, { "ջ", "j" }, { "Ջ", "J" },
        { "ռ", "r" }, { "Ռ", "R" }, { "ս", "s" }, { "Ս", "S" }, { "վ", "v" }, { "Վ", "V" },
        { "տ", "t" }, { "Տ", "T" }, { "ր", "r" }, { "Ր", "R" }, { "ց", "c" }, { "Ց", "C" },
        { "ու", "u" }, { "ՈՒ", "U" }, { "Ու", "U" }, { "փ", "p" }, { "Փ", "P" }, { "ք", "q" },
        { "Ք", "Q" }, { "օ", "o" }, { "Օ", "O" }, { "ֆ", "f" }, { "Ֆ", "F" }, { "և", "yev" }
    };

    public static readonly IReadOnlyDictionary<string, string> FarsiMapping = new Dictionary<string, string>
    {
        { "چ", "ch" }, { "ک", "k" }, { "گ", "g" }, { "پ", "p" }, { "ژ", "zh" }, { "ی", "y" }
    };

    public static readonly IReadOnlyDictionary<string, string> HebrewMapping = new Dictionary<string, string>
    {
        { "א", "a" }, { "ב", "b" }, { "ג", "g" }, { "ד", "d" }, { "ה", "h" }, { "ו", "v" },
        { "ז", "z" }, { "ח", "ch" }, { "ט", "t" }, { "י", "y" }, { "כ", "k" }, { "ך", "kh" },
        { "ל", "l" }, { "ם", "m" }, { "מ", "m" }, { "ן", "n" }, { "נ", "n" }, { "ס", "s" },
        { "ע", "a" }, { "פ", "f" }, { "ף", "ph" }, { "צ", "ts" }, { "ץ", "ts" }, { "ק", "k" },
        { "ר", "r" }, { "ש", "sh" }, { "ת", "t" }
    };

    public static readonly IReadOnlyDictionary<string, string> CharacterMapping = new Dictionary<string, string>()
        .Concat(CyrillicMapping)
        .Concat(GreekMapping)
        .Concat(ArabicMapping)
        .Concat(ArmenianMapping)
        .Concat(FarsiMapping)
        .Concat(HebrewMapping)
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}