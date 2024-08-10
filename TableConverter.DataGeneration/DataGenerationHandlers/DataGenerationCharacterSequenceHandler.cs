using TableConverter.DataGeneration.DataGenerationOptions;
using TableConverter.DataGeneration.DataModels;

namespace TableConverter.DataGeneration.DataGenerationHandlers
{
    public class DataGenerationCharacterSequenceHandler : DataGenerationTypeHandlerAbstract<DataGenerationCharacterSequenceOptions>
    {
        protected override string[] GenerateDataOverride(int rows, DataGenerationCharacterSequenceOptions? options, ushort blanks_percentage)
        {
            string[] data = new string[rows];

            for (int i = 0; i < rows; i++)
            {
                data[i] = CheckBlank(() => GenerateSequence(options!.CharacterSequence), blanks_percentage);
            }

            return data;
        }

        private string GenerateSequence(string sequence)
        {
            string result = "";

            Random random = new Random();

            foreach (char c in sequence)
            {
                switch (c)
                {
                    case '#':
                        result += random.Next(0, 10).ToString();
                        break;
                    case '@':
                        result += (char)random.Next(65, 91);
                        break;
                    case '^':
                        result += (char)random.Next(97, 123);
                        break;
                    case '?':
                        result += (char)random.Next(33, 48);
                        break;
                    case '*':
                        if (random.Next(0, 2) == 0)
                        {
                            result += random.Next(0, 10).ToString();
                        }
                        else
                        {
                            result += (char)random.Next(65, 123);
                        }
                        break;
                    case '$':
                        if (random.Next(0, 2) == 0)
                        {
                            result += random.Next(0, 10).ToString();
                        }
                        else
                        {
                            result += (char)random.Next(65, 91);
                        }
                        break;
                    case '%':
                        if (random.Next(0, 2) == 0)
                        {
                            result += random.Next(0, 10).ToString();
                        }
                        else
                        {
                            result += (char)random.Next(97, 123);
                        }
                        break;
                    default:
                        result += c;
                        break;
                }
            }

            return result;
        }
    }
}
