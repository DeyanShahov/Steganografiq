using System.Drawing;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Поддържани функционалности:");
        Console.WriteLine("-o [път до изходната папка] - пълната пътека до изходната папка, ако не се посочи, се приема текущата.");
        Console.WriteLine("-i [път до входния файл] - пълната пътека до входния файл, задължителен аргумент.");
        Console.WriteLine("-e [съобщение] - криптира даденото съобщение.");
        Console.WriteLine("-d [име на файл] - декриптира съобщение от зададения файл.");

        string outputFolder = Directory.GetCurrentDirectory();
        string inputFile = null;
        string action = null;
        string message = null;
        string outputFileName = null;

        //Loop za vavejdane na vhodnite danni, dokato ne se poluchi komanda za encode ili decode,
        //v sluchaq action ravno ne neshto
        while (action == null)
        {
            Console.Write("Изберете опция: ");
            string option = Console.ReadLine();

            switch (option)
            {
                case "o":
                    Console.Write("Въведете път: ");
                    outputFolder = Console.ReadLine();
                    break;
                case "i":
                    Console.Write("Въведете път: ");
                    inputFile = Console.ReadLine();
                    break;
                case "e":
                    action = "encode";
                    Console.Write("Въведете съобщение: ");
                    message = Console.ReadLine();
                    break;
                case "d":
                    action = "decode";
                    Console.Write("Въведете име на изходния фаил: ");
                    outputFileName = Console.ReadLine();
                    break;
                default:
                    Console.WriteLine("Грешка: Невалидна опция: " + option);
                    break;
            }
        }

        //Proverqva dali ima posochen fail
        if (string.IsNullOrEmpty(inputFile))
        {
            Console.WriteLine("Грешка: Не е посочен входен файл.");
            return;
        }

        //Proverqva dali posocheniq pat realno vodi kam neshto
        if (!File.Exists(inputFile))
        {
            Console.WriteLine($"Грешка: Файлът {inputFile} не съществува.");
            return;
        }

        // Proverqva dali posochenata izhodna papka sashtestvuva
        if (!Directory.Exists(outputFolder))
        {
            Console.WriteLine($"Изходната папка не съществува.");
            return;
        }

        //Proverka za sigurnost dali e izbrano nqkakvo deistvie
        if (action == null)
        {
            Console.WriteLine("Грешка: Не е посочено действие.");
            return;
        }


        //Proverka pri komanda encode dali saobshtenieto otgovarq na kriteriite
        if (action == "encode")
        {
            if (string.IsNullOrEmpty(message))
            {
                Console.WriteLine("Грешка: Не е посочено съобщение за криптиране.");
                return;
            }

            if (message.Length < 2)
            {
                Console.WriteLine("Грешка: Съобщението за кодиране трябва да съдържа поне 2 символа");
                return;
            }

            //Zarejdam izbranoto izobrajenie (png) v pametta
            Bitmap image = new Bitmap(inputFile);
            
            // Proverka dali izobrajenieto moje da sabere saobshtenieto
            if (message.Length * 8 + 96 > image.Width * image.Height * 3)
            {
                Console.WriteLine("Грешка: Изображението е твърде малко за кодиране на това съобщение.");
                return;
            }

            //Izvikvam funkciqta za encodvaneto na saobshtenieto v izobrajenieto
            Bitmap encodedImage = EncodeMessage(image, message);

            //Zapametqvane na encodnatoto izobrajenie v izbranata direktoriq
            encodedImage.Save(outputFolder + @"\SecredImage.png");

        }

        if (action == "decode")
        {
            if (string.IsNullOrEmpty(outputFileName))
            {
                Console.WriteLine("Грешка: Не е зададено име на изходен файл");
                return;
            }

            //Zarejdam izbranoto izobrajenie (png) v pametta
            Bitmap image = new Bitmap(inputFile);

            message = DecodeMessage(image);

            outputFileName = Path.ChangeExtension(outputFileName, ".txt");
            string outputFile = Path.Combine(outputFolder, outputFileName);

            File.WriteAllText(outputFile, message);
        }

        Console.WriteLine("Избрано действие: " + action);
        Console.WriteLine("Път до изходната папка: " + outputFolder);
        Console.WriteLine("Път до входния файл: " + inputFile);

        if (action == "encode")
        {
            Console.WriteLine("Съобщение за криптиране: " + message);
            Console.WriteLine($"Съобщението е успешно криптирано и записано");
        }
        else if (action == "decode")
        {
            Console.WriteLine("Име на изходен файл: " + outputFileName);
            Console.WriteLine($"Съобщението е: " + message);
        }
    }

    static Bitmap EncodeMessage(Bitmap image, string message)
    {
        // Add start marker to message
        message = "<START>" + message;
        // Add end marker to message
        message += "<END>";
       
        // Convert message to binary
        byte[] messageBytes = System.Text.Encoding.ASCII.GetBytes(message);
        string binaryMessage = "";
        foreach (byte b in messageBytes)
        {
            binaryMessage += Convert.ToString(b, 2).PadLeft(8, '0');
        }
        // Encode binary message into image
        Bitmap encodedImage = new Bitmap(image);
        int messageIndex = 0;
        for (int y = 0; y < encodedImage.Height; y++)
        {
            for (int x = 0; x < encodedImage.Width; x++)
            {
                Color pixel = encodedImage.GetPixel(x, y);
                if (messageIndex < binaryMessage.Length)
                {
                    int red = pixel.R & 254 | int.Parse(binaryMessage[messageIndex].ToString());
                    messageIndex++;
                    if (messageIndex < binaryMessage.Length)
                    {
                        int green = pixel.G & 254 | int.Parse(binaryMessage[messageIndex].ToString());
                        messageIndex++;
                        if (messageIndex < binaryMessage.Length)
                        {
                            int blue = pixel.B & 254 | int.Parse(binaryMessage[messageIndex].ToString());
                            messageIndex++;
                            encodedImage.SetPixel(x, y, Color.FromArgb(pixel.A, red, green, blue));
                        }
                        else
                        {
                            encodedImage.SetPixel(x, y, Color.FromArgb(pixel.A, pixel.R, green, pixel.B));
                        }
                    }
                    else
                    {
                        encodedImage.SetPixel(x, y, Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B));
                    }
                }
                else
                {
                    encodedImage.SetPixel(x, y, pixel);
                }
            }
        }
        return encodedImage;
    }

    static string DecodeMessage(Bitmap image)
    {
        // Decode binary message from image
        string binaryMessage = "";
        bool IsEnd = false;

        for (int y = 0; y < image.Height; y++)
        {
            for (int x = 0; x < image.Width; x++)
            {
                Color pixel = image.GetPixel(x, y);
                binaryMessage += (pixel.R & 1).ToString();
                binaryMessage += (pixel.G & 1).ToString();
                binaryMessage += (pixel.B & 1).ToString();
                if (binaryMessage.EndsWith("00000000")) // End marker found
                {
                    binaryMessage = binaryMessage.Substring(0, binaryMessage.Length - 8 * 5); // Remove end marker                 
                    int startIndex = binaryMessage.IndexOf("00111100"); // Start marker                   
                    IsEnd = true;
                    if (startIndex >= 0)
                    {
                        binaryMessage = binaryMessage.Substring(startIndex + 8 * 7); // Remove start marker
                        break;
                    }
                }
            }
            if (IsEnd) break;
        }

        // Convert binary message to ASCII
        byte[] messageBytes = new byte[binaryMessage.Length / 8];
        for (int i = 0; i < messageBytes.Length; i++)
        {
            messageBytes[i] = Convert.ToByte(binaryMessage.Substring(i * 8, 8), 2);
        }
        string message = System.Text.Encoding.ASCII.GetString(messageBytes);
        return message;
    }
}
