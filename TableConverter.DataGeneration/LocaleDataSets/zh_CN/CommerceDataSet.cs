using System.Collections.Immutable;
using TableConverter.DataGeneration.DataModels;
using TableConverter.DataGeneration.Interfaces;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.LocaleDataSetsBase.Definitions;

namespace TableConverter.DataGeneration.LocaleDataSets.zh_CN;

public class CommerceDataSet : CommerceBase
{
    public override ImmutableArray<string> Department { get; } =
    [
        "主页",
        "书籍",
        "健康",
        "孩子",
        "宝宝",
        "工业",
        "工具",
        "户外",
        "服装",
        "杂货",
        "汽车",
        "游戏",
        "玩具",
        "珠宝",
        "电子",
        "电影",
        "电脑",
        "美丽",
        "花园",
        "运动",
        "鞋子",
        "音乐"
    ];

    public override CommerceProductNameDefinition ProductName { get; } = new(
        [
            "不可思议的",
            "东方的",
            "人体工程学的",
            "优雅的",
            "华丽的",
            "回收的",
            "圆滑的",
            "好吃",
            "定制的",
            "实用的",
            "小的",
            "已许可的",
            "手工制作的",
            "手工的",
            "无品牌的",
            "智能的",
            "现代的",
            "电子的",
            "精彩绝伦的",
            "精致的",
            "豪华的",
            "质朴的",
            "贼好用的",
            "通用的"
        ],
        [
            "冷冻",
            "塑料",
            "新鲜",
            "木制",
            "棉花",
            "橡胶",
            "混凝土",
            "花岗岩",
            "软",
            "金属",
            "钢",
            "青铜"
        ],
        [
            "培根",
            "奶酪",
            "帽子",
            "手套",
            "披萨",
            "桌子",
            "椅子",
            "毛巾",
            "汽车",
            "沙拉",
            "球",
            "电脑",
            "肥皂",
            "自行车",
            "薯条",
            "衬衫",
            "裤子",
            "金枪鱼",
            "键盘",
            "鞋子",
            "香肠",
            "鱼肉",
            "鸡肉",
            "鼠标"
        ]
    );

    public override ImmutableArray<ITemplatedValueBuilder<FakerBase, LocaleBase>> ProductDescription { get; } =
    [
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("Sony/索尼 XR-55A80EK 55英寸4K超清认知智能OLED安卓摄像头电视"),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("【2023新品官方旗舰正品】DERE戴睿笔记本电脑二合一新Surface Pro13平板商务办公学生教育超轻薄便携电脑本"),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("【新品享壕礼】vivo iQOO Z8x手机官方旗舰店新品上市官网正品学生大电池大内存手机iqoo z7 z7x"),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("【至高立省300元 赠数据线】vivo Y78新品全面屏游戏拍照学生5G智能手机大电池官方旗舰店老人机Y78+ Y77"),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("人体工学椅电脑椅家用宿舍学生学习椅舒适久坐办公座椅转椅书桌椅"),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("华为笔记本电脑MateBook X Pro 2023 13代酷睿版锐炬显卡14.2英寸3.1K原色触控屏超轻薄旗舰微绒典藏1943"),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("可选16G【M2芯片】Apple/苹果 MacBook Pro 13英寸笔记本电脑剪辑设计大学生办公专用正品分期24G"),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("台式电脑机械硬盘SATA串口320G 500G 1TB 2T 3TB 4TB支持游戏监控"),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("小米电视 Redmi A43 高清智能电视 43英寸液晶平板电视L43RA-RA"),
        new TemplatedValueBuilder<FakerBase, LocaleBase>()
            .SetTemplate("鼠标有线USB静音无声家用办公台式笔记本电脑家用商务电竞男")
    ];
}