using System.Collections.Immutable;

namespace TableConverter.DataGeneration.LocaleDataSets.zh_CN;

public class VehicleDataSet : en.VehicleDataSet
{
    public override ImmutableArray<string> BicycleType { get; } =
    [
        "三轮车",
        "健身自行车",
        "公路自行车",
        "冒险公路自行车",
        "卧式自行车",
        "双人自行车",
        "双运动自行车",
        "场地/固定齿轮自行车",
        "城市自行车",
        "小轮车自行车",
        "山地自行车",
        "巡洋舰自行车",
        "平足舒适自行车",
        "折叠自行车",
        "旅行自行车",
        "混合动力自行车",
        "越野自行车",
        "铁人三项/计时自行车"
    ];

    public override ImmutableArray<string> FuelType { get; } =
    [
        "柴油",
        "汽油",
        "混合动力",
        "电动"
    ];

    public override ImmutableArray<string> Manufacturer { get; } =
    [
        "三菱",
        "丰田",
        "保时捷",
        "克莱斯勒",
        "兰博基尼",
        "凯迪拉克",
        "劳斯莱斯",
        "吉普",
        "名爵",
        "塔塔",
        "大众",
        "奔驰",
        "奔驰smart",
        "奥迪",
        "宝马",
        "宾利",
        "布加迪",
        "捷豹",
        "斯巴鲁",
        "斯柯达",
        "日产",
        "本田",
        "极星",
        "标致",
        "比亚迪",
        "沃克斯豪尔",
        "沃尔沃",
        "法拉利",
        "特斯拉",
        "玛莎拉蒂",
        "现代",
        "福特",
        "菲亚特",
        "蔚来",
        "起亚",
        "路虎",
        "迷你",
        "道奇",
        "铃木",
        "阿斯顿·马丁",
        "雪佛兰",
        "雪铁龙",
        "雷诺",
        "马恒达",
        "马自达",
        "马鲁蒂"
    ];

    public override ImmutableArray<string> VehicleType { get; } =
    [
        "客车",
        "掀背车",
        "旅行车",
        "货车",
        "越野车",
        "轿车",
        "面包车"
    ];
}