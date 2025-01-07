using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.LocaleDataSetsBase.Definitions;

namespace TableConverter.DataGeneration.LocaleDataSets.zh_CN;

public class DateDataSet : DateBase
{
    public override DateEntryDefinition Month { get; } = new(
        [
            "一月",
            "七月",
            "三月",
            "九月",
            "二月",
            "五月",
            "八月",
            "六月",
            "十一月",
            "十二月",
            "十月",
            "四月"
        ],
        [
            "10月",
            "11月",
            "12月",
            "1月",
            "2月",
            "3月",
            "4月",
            "5月",
            "6月",
            "7月",
            "8月",
            "9月"
        ],
        [],
        []
    );

    public override DateEntryDefinition Weekday { get; } = new(
        [
            "星期一",
            "星期三",
            "星期二",
            "星期五",
            "星期六",
            "星期四",
            "星期天"
        ],
        [
            "周一",
            "周三",
            "周二",
            "周五",
            "周六",
            "周四",
            "周日"
        ],
        [],
        []
    );
}