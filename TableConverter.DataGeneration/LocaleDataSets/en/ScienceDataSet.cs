using System.Collections.Immutable;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.LocaleDataSetsBase.Definitions;

namespace TableConverter.DataGeneration.LocaleDataSets.en;

public class ScienceDataSet : ScienceBase
{
    public override ImmutableArray<ScienceUnitDefinition> Unit { get; } =
    [
        new("meter", "m"),
        new("second", "s"),
        new("mole", "mol"),
        new("ampere", "A"),
        new("kelvin", "K"),
        new("candela", "cd"),
        new("kilogram", "kg"),
        new("radian", "rad"),
        new("hertz", "Hz"),
        new("newton", "N"),
        new("pascal", "Pa"),
        new("joule", "J"),
        new("watt", "W"),
        new("coulomb", "C"),
        new("volt", "V"),
        new("ohm", "Ω"),
        new("tesla", "T"),
        new("degree Celsius", "°C"),
        new("lumen", "lm"),
        new("becquerel", "Bq"),
        new("gray", "Gy"),
        new("sievert", "Sv"),
        new("steradian", "sr"),
        new("farad", "F"),
        new("siemens", "S"),
        new("weber", "Wb"),
        new("henry", "H"),
        new("lux", "lx"),
        new("katal", "kat")
    ];

    public override ImmutableArray<ScienceChemicalElementDefinition> ChemicalElement { get; } =
    [
        new("Hydrogen", "H", 1),
        new("Helium", "He", 2),
        new("Lithium", "Li", 3),
        new("Beryllium", "Be", 4),
        new("Boron", "B", 5),
        new("Carbon", "C", 6),
        new("Nitrogen", "N", 7),
        new("Oxygen", "O", 8),
        new("Fluorine", "F", 9),
        new("Neon", "Ne", 10),
        new("Sodium", "Na", 11),
        new("Magnesium", "Mg", 12),
        new("Aluminium", "Al", 13),
        new("Silicon", "Si", 14),
        new("Phosphorus", "P", 15),
        new("Sulfur", "S", 16),
        new("Chlorine", "Cl", 17),
        new("Argon", "Ar", 18),
        new("Potassium", "K", 19),
        new("Calcium", "Ca", 20),
        new("Scandium", "Sc", 21),
        new("Titanium", "Ti", 22),
        new("Vanadium", "V", 23),
        new("Chromium", "Cr", 24),
        new("Manganese", "Mn", 25),
        new("Iron", "Fe", 26),
        new("Cobalt", "Co", 27),
        new("Nickel", "Ni", 28),
        new("Copper", "Cu", 29),
        new("Zinc", "Zn", 30),
        new("Gallium", "Ga", 31),
        new("Germanium", "Ge", 32),
        new("Arsenic", "As", 33),
        new("Selenium", "Se", 34),
        new("Bromine", "Br", 35),
        new("Krypton", "Kr", 36),
        new("Rubidium", "Rb", 37),
        new("Strontium", "Sr", 38),
        new("Yttrium", "Y", 39),
        new("Zirconium", "Zr", 40),
        new("Niobium", "Nb", 41),
        new("Molybdenum", "Mo", 42),
        new("Technetium", "Tc", 43),
        new("Ruthenium", "Ru", 44),
        new("Rhodium", "Rh", 45),
        new("Palladium", "Pd", 46),
        new("Silver", "Ag", 47),
        new("Cadmium", "Cd", 48),
        new("Indium", "In", 49),
        new("Tin", "Sn", 50),
        new("Antimony", "Sb", 51),
        new("Tellurium", "Te", 52),
        new("Iodine", "I", 53),
        new("Xenon", "Xe", 54),
        new("Caesium", "Cs", 55),
        new("Barium", "Ba", 56),
        new("Lanthanum", "La", 57),
        new("Cerium", "Ce", 58),
        new("Praseodymium", "Pr", 59),
        new("Neodymium", "Nd", 60),
        new("Promethium", "Pm", 61),
        new("Samarium", "Sm", 62),
        new("Europium", "Eu", 63),
        new("Gadolinium", "Gd", 64),
        new("Terbium", "Tb", 65),
        new("Dysprosium", "Dy", 66),
        new("Holmium", "Ho", 67),
        new("Erbium", "Er", 68),
        new("Thulium", "Tm", 69),
        new("Ytterbium", "Yb", 70),
        new("Lutetium", "Lu", 71),
        new("Hafnium", "Hf", 72),
        new("Tantalum", "Ta", 73),
        new("Tungsten", "W", 74),
        new("Rhenium", "Re", 75),
        new("Osmium", "Os", 76),
        new("Iridium", "Ir", 77),
        new("Platinum", "Pt", 78),
        new("Gold", "Au", 79),
        new("Mercury", "Hg", 80),
        new("Thallium", "Tl", 81),
        new("Lead", "Pb", 82),
        new("Bismuth", "Bi", 83),
        new("Polonium", "Po", 84),
        new("Astatine", "At", 85),
        new("Radon", "Rn", 86),
        new("Francium", "Fr", 87),
        new("Radium", "Ra", 88),
        new("Actinium", "Ac", 89),
        new("Thorium", "Th", 90),
        new("Protactinium", "Pa", 91),
        new("Uranium", "U", 92),
        new("Neptunium", "Np", 93),
        new("Plutonium", "Pu", 94),
        new("Americium", "Am", 95),
        new("Curium", "Cm", 96),
        new("Berkelium", "Bk", 97),
        new("Californium", "Cf", 98),
        new("Einsteinium", "Es", 99),
        new("Fermium", "Fm", 100),
        new("Mendelevium", "Md", 101),
        new("Nobelium", "No", 102),
        new("Lawrencium", "Lr", 103),
        new("Rutherfordium", "Rf", 104),
        new("Dubnium", "Db", 105),
        new("Seaborgium", "Sg", 106),
        new("Bohrium", "Bh", 107),
        new("Hassium", "Hs", 108),
        new("Meitnerium", "Mt", 109),
        new("Darmstadtium", "Ds", 110),
        new("Roentgenium", "Rg", 111),
        new("Copernicium", "Cn", 112),
        new("Nihonium", "Nh", 113),
        new("Flerovium", "Fl", 114),
        new("Moscovium", "Mc", 115),
        new("Livermorium", "Lv", 116),
        new("Tennessine", "Ts", 117),
        new("Oganesson", "Og", 118)
    ];
}