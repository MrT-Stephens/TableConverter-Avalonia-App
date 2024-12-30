using TableConverter.DataGeneration;
using TableConverter.DataGeneration.LocaleDataSetsBase;
using TableConverter.DataGeneration.Modules;
using TableConverter.DataModels;

namespace TableConverter.Services.DataGenerationAttributedModules;

[DataGenerationModule("Person", 
    "Module for generating personal information such as names, gender, biography, etc.",
    "DataGenerationPersonIcon")]
public class PersonAttributedModule(FakerBase faker, LocaleBase locale, Randomizer randomizer)
    : PersonModule(faker, locale, randomizer)
{
    [DataGenerationModuleMethod("Full Name",
        "Generates a full name for a person. Example: 'John Michael Doe'. Could include title, suffix, or middle name.")]
    public override string FullName()
    {
        return base.FullName();
    }

    [DataGenerationModuleMethod("First Name", "Generates the first name for a person. Example: 'John'",
        "Returns a first name based on the specified sex (e.g., Male or Female).")]
    public override string FirstName(SexEnum sex = SexEnum.Generic)
    {
        return base.FirstName(sex);
    }

    [DataGenerationModuleMethod("Middle Name", "Generates the middle name for a person. Example: 'Michael'",
        "Returns a middle name based on the specified sex.")]
    public override string MiddleName(SexEnum sex = SexEnum.Generic)
    {
        return base.MiddleName(sex);
    }

    [DataGenerationModuleMethod("Last Name", "Generates the last name for a person. Example: 'Doe'",
        "Returns a last name based on the specified sex.")]
    public override string LastName(SexEnum sex = SexEnum.Generic)
    {
        return base.LastName(sex);
    }

    [DataGenerationModuleMethod("Title", "Generates a title for a person. Example: 'Dr.', 'Mr.', 'Ms.'",
        "Returns a title based on the specified sex.")]
    public override string Title(SexEnum sex = SexEnum.Generic)
    {
        return base.Title(sex);
    }

    [DataGenerationModuleMethod("Sex", "Generates the sex of a person. Example: 'Male' or 'Female'",
        "Returns the sex of a person in the specified format.")]
    public override string Sex(SexFormat format = SexFormat.UpperFullWord)
    {
        return base.Sex(format);
    }

    [DataGenerationModuleMethod("Suffix", "Generates a suffix for a person. Example: 'Jr.', 'Sr.', 'III'",
        "Returns a suffix based on the specified sex.")]
    public override string Suffix(SexEnum sex = SexEnum.Generic)
    {
        return base.Suffix(sex);
    }

    [DataGenerationModuleMethod("Gender",
        "Generates the gender of a person. Example: 'Non-binary', 'Male', or 'Female'")]
    public override string Gender()
    {
        return base.Gender();
    }

    [DataGenerationModuleMethod("Biography",
        "Generates a biography for a person. Example: 'John Doe is a software engineer from San Francisco, CA.'")]
    public override string Biography()
    {
        return base.Biography();
    }

    [DataGenerationModuleMethod("Job Type",
        "Generates a job type for a person. Example: 'Software Engineer', 'Artist', or 'Doctor'")]
    public override string JobType()
    {
        return base.JobType();
    }
}