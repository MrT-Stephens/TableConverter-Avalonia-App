using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.Tests.FakerTests;

public class FakerSeededGenerationTest(Faker faker) : IClassFixture<Faker>
{
    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void TestFaker_WithSeededGeneration(int seed, string locale, TableData expected)
    {
        // Set the seed and locale
        faker.Seed(seed);
        faker.LocaleType = locale;

        // Generate the data
        var data = Faker.Create(faker)
            .Add("First Name", f => f.Person.FirstName())
            .Add("Last Name", f => f.Person.LastName())
            .Add("Phone Number", f => f.Phone.PhoneNumber())
            .Add("Email", f => f.Internet.Email())
            .Add("City", f => f.Location.City())
            .Add("Country", f => f.Location.Country())
            .Add("Words", f => f.Word.Words())
            .WithRowCount(25)
            .Build();

        // Assert to check if the data is generated correctly
        Assert.Equal(expected, data);
    }

    [Theory]
    [MemberData(nameof(GetTestCases))]
    public void TestFakerAsync_WithSeededGeneration(int seed, string locale, TableData expected)
    {
        // Set the seed and locale
        faker.Seed(seed);
        faker.LocaleType = locale;

        // Generate the data
        var data = Faker.Create(faker)
            .Add("First Name", f => f.Person.FirstName())
            .Add("Last Name", f => f.Person.LastName())
            .Add("Phone Number", f => f.Phone.PhoneNumber())
            .Add("Email", f => f.Internet.Email())
            .Add("City", f => f.Location.City())
            .Add("Country", f => f.Location.Country())
            .Add("Words", f => f.Word.Words())
            .WithRowCount(25)
            .Build();

        // Assert to check if the data is generated correctly
        Assert.Equal(expected, data);
    }

    public static IEnumerable<object[]> GetTestCases()
    {
        List<(int Seed, string Locale, TableData ExpectedData)> cases =
        [
            (
                1045453543,
                "en",
                new TableData(
                    ["First Name", "Last Name", "Phone Number", "Email", "City", "Country", "Words"],
                    [
                        new[]
                        {
                            "Carmelo", "Mante", "887-792-7907 x01947", "Rozella_Hackett@gmail.com", "Charlieside",
                            "Canada", "cumbersome for afore whenever disk pro invite fork following"
                        },
                        new[]
                        {
                            "Sandrine", "Mraz", "(349) 160-5512 x79877", "Cornelius-Blanda81@yahoo.com", "Lake Rhea",
                            "Eswatini", "yahoo individual fabricate corny promptly as yahoo within yuck among"
                        },
                        new[]
                        {
                            "Dariana", "O\"Keefe", "665-792-2273 x2384", "Annalise61@gmail.com", "Mavisboro",
                            "Martinique", "joyously irresponsible unless bah than"
                        },
                        new[]
                        {
                            "Valentine", "Olson", "113.553.9543 x1084", "Marcia95@hotmail.com", "Metaberg",
                            "Luxembourg", "tool aside catalog dispense platter house yuck"
                        },
                        new[]
                        {
                            "June", "Spencer", "1-892-145-2553 x1750", "Hubert-Hyatt38@hotmail.com", "West Kaitlin",
                            "Saint Lucia", "speedily overheard pupil off saloon whether though"
                        },
                        new[]
                        {
                            "Jonas", "Stanton", "516.957.4596", "Ola_Herzog@gmail.com", "Elk Grove", "Sint Maarten",
                            "provided ouch shampoo"
                        },
                        new[]
                        {
                            "Maximillia", "O\"Conner", "151-922-6986", "Hassan-Franecki97@hotmail.com",
                            "Lake Emiliestead", "Wallis and Futuna",
                            "boohoo enormously ha fumigate yet vain serialize upwardly"
                        },
                        new[]
                        {
                            "Norma", "Boyer", "1-847-132-7501", "Lionel68@gmail.com", "Fort Guy", "Ukraine",
                            "yippee aha versus dearly"
                        },
                        new[]
                        {
                            "Emilie", "Kuhlman", "768-982-3172 x452", "Lindsay_Rutherford64@hotmail.com", "Haagberg",
                            "Congo", "courageously around jive doubter vol striking"
                        },
                        new[]
                        {
                            "Emmie", "Ebert", "645.633.8330", "Linwood-Romaguera89@yahoo.com", "West Melissa",
                            "Afghanistan", "jeopardise among sock exalted"
                        },
                        new[]
                        {
                            "Josiane", "Weimann", "534.069.9230 x9428", "Juliet.Torphy@yahoo.com", "East Pattieburgh",
                            "Guyana", "via tremendously tighten sheepishly"
                        },
                        new[]
                        {
                            "Guy", "Rice", "215-254-6572 x36322", "Monique.McLaughlin29@gmail.com", "Fort Neha",
                            "Cameroon", "searchingly at posh veto between geez annually fooey vision"
                        },
                        new[]
                        {
                            "Wilma", "Mohr", "429.028.1449 x96812", "Meghan62@hotmail.com", "Fort Norwoodville", "Peru",
                            "handle qua opposite"
                        },
                        new[]
                        {
                            "Marion", "Daugherty", "(109) 347-8298 x13448", "Joseph-Olson28@hotmail.com",
                            "North Frankview", "Bermuda",
                            "orientate promptly amused duh tenant apropos athwart yum whether"
                        },
                        new[]
                        {
                            "Casandra", "Hintz", "277-213-8180 x2728", "Bernita.Bernier@hotmail.com", "Vernerstead",
                            "Benin", "knife ew fellow inasmuch owlishly carelessly flint abaft"
                        },
                        new[]
                        {
                            "Bartholome", "Conroy", "314-152-7602", "Karlie_Veum@yahoo.com", "Port Ruben",
                            "United States of America", "what spice pike flawed amidst which scarification numb label"
                        },
                        new[]
                        {
                            "Idella", "Keeling", "259-752-9263 x148", "Beaulah.Monahan@yahoo.com", "Tristianfort",
                            "Cyprus", "floss vestment eek gallery deed monocle even faithfully nun than"
                        },
                        new[]
                        {
                            "Isaac", "Stiedemann", "166.253.9032 x7192", "Muhammad50@gmail.com", "Lake Bertha",
                            "Democratic People\"s Republic of Korea", "ugh of given requite ha than usefully"
                        },
                        new[]
                        {
                            "Josefa", "Okuneva", "643-384-0391 x8845", "Nannie71@yahoo.com", "Mertieville", "Bahrain",
                            "repeatedly trek spectacles crumble phew likewise"
                        },
                        new[]
                        {
                            "Pauline", "Stamm", "(601) 584-8269", "Brayan-Will87@gmail.com", "Union City",
                            "Norfolk Island", "ack entice cycle unnaturally restaurant before quaintly"
                        },
                        new[]
                        {
                            "Jeff", "Monahan", "538.847.0996 x50161", "Lolita_Lehner79@yahoo.com", "North Raquelfurt",
                            "Nigeria", "knit accessorise wilted steep confound usually elementary yet jacket"
                        }
                    ]
                )
            ),
            (
                1045453543,
                "zh_CN",
                new TableData(
                    ["First Name", "Last Name", "Phone Number", "Email", "City", "Country", "Words"],
                    [
                        new[]
                        {
                            "嘉懿", "濮", "18877927907", "ggqk1z-vcr53@sina.com", "{海}{州市}", "South Sudan", "搔 差点儿 a 倒"
                        },
                        new[]
                        {
                            "绍齐", "巩", "0277-54837488", "m8eiuk.kj744@yeah.net", "{包}{徽市}", "Christmas Island",
                            "拉 pro cruelty 冷 through till till against"
                        },
                        new[]
                        {
                            "振家", "栾", "0493-01167253", "v9ru72-lib@126.com", "{珠}{乡县}", "Myanmar",
                            "worth 总共 猛然 hence while than 全部"
                        },
                        new[]
                        {
                            "俊驰", "和", "0620-52223043", "k5q_nz151@hotmail.com", "{宁}{海市}",
                            "Bonaire, Sint Eustatius and Saba", "fellow 挖 indeed 全 given 掷 少"
                        },
                        new[]
                        {
                            "鹏", "锐", "0145-45697291", "id7mft-p1f@vip.qq.com", "{武}{安市}", "Timor-Leste",
                            "掐 简直 ew eek even midst 熟练 aha to"
                        },
                        new[]
                        {
                            "馥君", "建", "0579-51756777", "lm1md2-qap@qq.com", "{北}{都市}", "Dominican Republic",
                            "worth phew provided 捧"
                        },
                        new[]
                        {
                            "耀杰", "万俟", "0518-20880455", "vacsbt-r2q@hotmail.com", "{南}{林市}", "Ukraine",
                            "translation 捧 woot 差点儿"
                        },
                        new[]
                        {
                            "鹏煊", "厉", "0355-92651214", "vdak5x-isl3@hotmail.com", "{南}{都市}",
                            "Saint Pierre and Miquelon", "boohoo 撞 chairperson"
                        },
                        new[]
                        {
                            "建辉", "琦", "066-64661748", "ui8.tmd@126.com", "{西}{门市}", "Cameroon",
                            "given inasmuch of with 扶"
                        },
                        new[]
                        {
                            "呈轩", "城", "0266-79469963", "i1crqi26@sina.com", "{诸}{州市}", "El Salvador",
                            "清楚 besides 咀 搀 jungle gee inasmuch concerning and than"
                        },
                        new[]
                        {
                            "癸霖", "郝", "0327-70319481", "rydkjv51@21cn.com", "{福}{徽市}", "Kyrgyz Republic",
                            "蠕动 必然 全 ah out pish boohoo"
                        },
                        new[]
                        {
                            "明", "丰", "12237179288", "i1csbt_prj19@hotmail.com", "{厦}{原市}", "Ecuador",
                            "legend as though hence 难道 near"
                        },
                        new[]
                        {
                            "嘉熙", "顾", "097-31172772", "m97k5q-oxt@outlook.com", "{上}{徽市}", "Moldova",
                            "撑 elver 披 稍微 血淋淋 amongst"
                        },
                        new[]
                        {
                            "昊天", "霍", "11267771607", "ui870@139.com", "{安}{码市}", "Senegal",
                            "willow 光 apud 全部 now the 十分"
                        },
                        new[]
                        {
                            "嘉熙", "昂", "11568729310", "k5q.vix79@126.com", "{海}{京市}", "Jamaica",
                            "摇 很 也许 incidentally for 一定 however"
                        },
                        new[]
                        {
                            "志强", "翠", "038-18027287", "fozmy2-ui443@163.com", "{济}{京市}", "Burundi",
                            "yippee 逐渐 incidentally furthermore ew 平常 whenever 生动 鲁莽 坏"
                        },
                        new[]
                        {
                            "擎宇", "摩", "0276-02659564", "knnk88-iwd41@yahoo.com.cn", "{宁}{海市}", "Sudan",
                            "血淋淋 amidst which scarification 苦 label 聪明"
                        },
                        new[]
                        {
                            "昊然", "廉", "0975-29263148", "fvpts4.n5x@vip.qq.com", "{贵}{海市}", "Cyprus",
                            "摘 vestment eek gallery deed monocle 一概 一味 nun than"
                        },
                        new[]
                        {
                            "明哲", "达", "11662539032", "hm1nuy.tqo73@163.com", "{武}{口市}", "Guatemala",
                            "interior psst ugh"
                        },
                        new[]
                        {
                            "炎彬", "梁", "0473-61895465", "mg0iuc.kle@hotmail.com", "{贵}{京市}",
                            "Svalbard & Jan Mayen Islands", "搔 挖 猛然 after ha 更加 红"
                        },
                        new[]
                        {
                            "天翊", "鱼", "18914724673", "knnk88.g7l@vip.qq.com", "{西}{头市}", "Vietnam",
                            "撞 拔 excluding 攥"
                        },
                        new[] { "煜城", "清", "18262186731", "lzbmd260@sina.com", "{太}{码市}", "Latvia", "操作 lotion 细" },
                        new[]
                        {
                            "博超", "睦", "0477-42086163", "vacsbt.kfc10@foxmail.com", "{长}{南市}", "Tonga",
                            "yet jacket story airline"
                        },
                        new[]
                        {
                            "婷方", "通", "16092575526", "ggqk1z_nj3@yahoo.cn", "{长}{沙市}", "Bahamas",
                            "geez 劈 atop 必然 就 抱 黑不溜秋 几乎 than 捶"
                        },
                        new[]
                        {
                            "子默", "折", "0846-18023055", "k7znlr-jen@yahoo.com.cn", "{成}{徽市}", "Finland",
                            "平坦 if since"
                        }
                    ]
                )
            ),
            (
                1045453543,
                "en_GB",
                new TableData(
                    ["First Name", "Last Name", "Phone Number", "Email", "City", "Country", "Words"],
                    [
                        new[]
                        {
                            "Carmelo", "Mante", "016977 8877", "Demetrius73@hotmail.com", "Kuhn Court", "Suriname",
                            "intent wherever concentration"
                        },
                        new[]
                        {
                            "Myrtice", "Schimmel", "011327 03762", "Monty-Metz35@yahoo.com", "Stromanworth",
                            "Republic of Korea", "um quaintly messy conversation shrilly"
                        },
                        new[]
                        {
                            "Raven", "Reynolds", "0101 688 7836", "Ashlynn-Grady@gmail.com", "Upton Halvorson",
                            "Burkina Faso", "as yahoo within yuck among density purse"
                        },
                        new[]
                        {
                            "Kayley", "Nicolas", "016977 9222", "Fae-Fisher@gmail.com", "Bahringerley", "Congo",
                            "embarrassment noted freely joyously irresponsible unless bah"
                        },
                        new[]
                        {
                            "Leone", "Wiegand", "0837 113 5539", "Hazle.Hermann@gmail.com", "Beatty Gardens",
                            "Uzbekistan", "ha knowingly aha gadzooks scruple wetly woeful scarcely tribe nor"
                        },
                        new[]
                        {
                            "Yolanda", "Johns", "0500 789214", "Devonte.Macejkovic16@yahoo.com", "High Kertzmannwood",
                            "Sudan", "incidentally provided ouch obsess speedily overheard pupil off saloon whether"
                        },
                        new[]
                        {
                            "Madyson", "Langworth", "01425 16957", "Katelynn-Wyman@yahoo.com", "Old McCullough",
                            "Svalbard & Jan Mayen Islands", "through innocent psst"
                        },
                        new[]
                        {
                            "Winfield", "Jacobson", "0116 001 5192", "Lucious_Walsh@hotmail.com", "Waters Gardens",
                            "Central African Republic", "since um repeatedly boohoo enormously ha fumigate yet"
                        },
                        new[]
                        {
                            "Tia", "Schamberger", "0500 170051", "Hilton_Quigley25@gmail.com", "St. Ornwick", "Jersey",
                            "successfully while whoever bide loyally hornet whereas construe"
                        },
                        new[]
                        {
                            "German", "Shanahan", "023 7689 8231", "Dangelo.Kovacek@yahoo.com", "Kuphalwick", "Tuvalu",
                            "eventually mortally pfft tense unimpressively"
                        },
                        new[]
                        {
                            "Marianna", "Miller", "018838 29264", "Lessie-Huel@gmail.com", "Long Romaguera",
                            "Saint Martin", "gerbil seemingly a even jeopardise among sock exalted misjudge wasteful"
                        },
                        new[]
                        {
                            "Nedra", "Herman", "016992 30942", "Marcella_Legros@hotmail.com", "Armstrongdon", "Curacao",
                            "anti via tremendously tighten sheepishly lest"
                        },
                        new[]
                        {
                            "Mustafa", "Sawayn", "01525 465723", "Etha_Franey@gmail.com", "East Wizabury",
                            "Virgin Islands, U.S.", "clearly athwart resolve always polyester"
                        },
                        new[]
                        {
                            "Randi", "Lindgren", "01052 594966", "Tyrell8@gmail.com", "Lower Jast Gardens",
                            "Mozambique", "reassuringly rosemary amidst"
                        },
                        new[]
                        {
                            "Anais", "Cummerata", "016977 1371", "Louvenia-Jenkins@hotmail.com", "Yostfield", "Somalia",
                            "facilitate ouch behind"
                        },
                        new[]
                        {
                            "Rhiannon", "Halvorson", "055 8156 8729", "Clifton99@gmail.com", "Weissnatthorpe",
                            "Montserrat", "duh tenant apropos"
                        },
                        new[]
                        {
                            "Dax", "Haag", "01413 32527", "Daren-Carroll17@gmail.com", "High Doyle End",
                            "Bonaire, Sint Eustatius and Saba",
                            "upliftingly mmm oof backbone crafty tooth yippee rudely incidentally furthermore"
                        },
                        new[]
                        {
                            "Dora", "Osinski", "016977 0714", "Fiona41@gmail.com", "High Nicolasham", "Norfolk Island",
                            "gadzooks psst hm toward whose sans what spice pike flawed"
                        },
                        new[]
                        {
                            "Bonnie", "Stroman", "016977 1555", "Idella34@gmail.com", "Ryanthorpe", "Tuvalu",
                            "consequently uh-huh alongside pendant busily pfft tusk barring"
                        },
                        new[]
                        {
                            "Dora", "Trantow", "0895 553 6275", "Corene_Feest61@gmail.com", "Upper Schaefer",
                            "Gibraltar", "parsnip needily why"
                        },
                        new[]
                        {
                            "Emelie", "Rath", "0800 227945", "Deon-Barton@gmail.com", "Lower King",
                            "Sao Tome and Principe",
                            "zowie excluding godfather conservative twist drat makeover piglet hm"
                        },
                        new[]
                        {
                            "Ron", "Bernhard", "0500 188458", "Torey30@gmail.com", "Dach Common", "Burkina Faso",
                            "willow swathe vice mechanically"
                        },
                        new[]
                        {
                            "Eladio", "Moen", "028 5160 1584", "Ebba_Oberbrunner12@hotmail.com", "Swaniawski Hill",
                            "Tanzania", "thigh drat towards unless doubtfully pry despite"
                        },
                        new[]
                        {
                            "Danyka", "Schaden", "016195 38847", "Stuart2@yahoo.com", "Little Murphyhill", "Israel",
                            "fooey stylish cauliflower grandson without er celebrated absolve with"
                        },
                        new[]
                        {
                            "Dorris", "Baumbach", "029 5468 5071", "Samanta-Mosciski8@hotmail.com", "Nether Lockman",
                            "Congo", "potable of needily"
                        }
                    ]
                )
            )
        ];

        return cases.Select(val => new object[]
        {
            val.Seed,
            val.Locale,
            val.ExpectedData
        });
    }
}