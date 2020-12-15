using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace ImageToASCII
{
    class Program
    {
        /*
        Program zamyka się automatycznie po wygenerowaniu obrazka i zamknięciu MessageBoxa.
        ASCII-art zapisywany jest do pliku image.txt w tym samym folderze co plik exe.
        Można wybrać 1 z 3 zestawów znaków (osobiście polecam ten pierwszy).

        W notepad++ wyświetla się dobrze, w notepad nie (przynajmniej mi). Innych edytorów tekstu nie testowałem.
        Sprawdzona czcionka: Courier new. Ważne, żeby używana czcionka miała stałą szerokość znaków.
        Te czcionki w teorii powinny działać: https://en.wikipedia.org/wiki/List_of_monospaced_typefaces

        Jeśli ASCII-art jest nie równy, to najprawdopodobniej kwestia czcionki lub zawijania linijek.

        Wyjściowy obrazek może być trochę mniej szeroki niż oryginał, jest tak ze względu na to, iż większość czcionek 
        ma większą wysokość znaków niż ich szerokość.
        */

        // STAThread potrzebne do wykorzystania OpenFileDialog, stwierdziłem że nie ma sensu tworzyć Forma tylko z tego powodu
        [STAThread]
        static void Main(string[] args)
        {
            //Zestawy znaków, jeden standardowy, drugi dla negatywu
            string[] charSets = new string[] { "@#$%&?<=+!;-*^, ", "$@B%8&WM#*oahkbdpqwmZO0QLCJUYXzcvunxrjft/\\|()1{}[]?-_+~<>i!lI;:,\"^`'. ", "@%#*+=-:. " };
            string selectedCharSet;

            //Dokładność obrazka (dokładniej to pierwiastek z ilości px przypadających na jeden znak)
            //1 to najdokładniejszy obrazek, im większa wartość tym mniej dokładny
            int res;

            //Dialog odpowiedzialny za wybieranie pliku
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Images|*.png;*.jpg;*.jpeg;*.jfif;*.tif;*.tiff;*.bmp";

            //Bitmapa (obrazek)
            Bitmap image = null;

            Console.WriteLine("Wybierz plik graficzny do przerobienia.");

            DialogResult dialogResult = ofd.ShowDialog();
            Console.WriteLine();

            //Tablica przechowujące kolory
            Color[,] grayscaleColorList;

            //Pobieranie pliku
            if (dialogResult == DialogResult.OK)
            {
                image = new Bitmap(ofd.FileName);
                MessageBox.Show("Załadowano plik.");
            } 
            else if (dialogResult == DialogResult.Cancel)
            {
                MessageBox.Show("Nie wybrano pliku.");
                Environment.Exit(0);
            }

            //Pobieram wybrany zestaw znaków
            selectedCharSet = charSets[int.Parse(GetAnswer("Wybierz zestaw znaków.", charSets)) - 1];

            //Pobieram dokładność i rozmiar obrazka
            res = GetAnswerInt("Podaj dokładność (a za razem rozmiar) obrazka w postaci liczby naturalnej.\n Im większa wartość tym mniejsza dokładność i rozmiar.", 1, image.Width * image.Height);

            grayscaleColorList = new Color[(int) Math.Ceiling(image.Width / (float)res),(int)Math.Ceiling(image.Height / (float)res)];

            //Przechodzę przez kolejne grupy pikseli, pobieram ich średni kolor oraz zamieniam ten kolor na czarno-biały
            for (int i = 0; i < image.Width; i += res)
            {
                for (int j = 0; j < image.Height; j += res)
                {
                    int sizeX = res;
                    int sizeY = res;

                    if (i + res > image.Width) sizeX = image.Width - i;
                    if (j + res > image.Height) sizeY = image.Height - j;
                    Color tempColor = GetAveragePixelColor(image, i, j, sizeX, sizeY);
                    int grayscale = (tempColor.R + tempColor.G + tempColor.B) / 3;
                    grayscaleColorList[i / res, j / res] = Color.FromArgb(grayscale, grayscale, grayscale);
                }
            }

            //Co ile odcieni szarości ma być nowy znak
            int charStep = 256 / selectedCharSet.Length;

            //Usuwanie istniejącego pliku, otwieranie StreamWritera do edycji pliku
            string filePath = Environment.CurrentDirectory + @"\image.txt";
            File.Delete(filePath);
            StreamWriter sw = File.AppendText(filePath);

            //Przechodzę po liście kolorów oraz wpisuję do pliku image.txt odpowiedni znak
            //Znak zależy od ciemności/jasności danego fragmentu obrazka
            for (int i = 0; i < grayscaleColorList.GetLength(1); i++)
            {
                for (int j = 0; j < grayscaleColorList.GetLength(0); j++)
                {
                    int index = grayscaleColorList[j, i].R / charStep;
                    if (index > selectedCharSet.Length - 1) sw.Write(selectedCharSet[selectedCharSet.Length - 1]);
                    else sw.Write(selectedCharSet[index]);
                }
                sw.WriteLine();
            }

            MessageBox.Show("Pomyślnie wygenerowano ASCII-art");
        }

        //Zwraca średni kolor pixeli o pozycji między (x, y) oraz (x + sizeX, y + sizeY)
        private static Color GetAveragePixelColor(Bitmap bitmap, int x, int y, int sizeX, int sizeY)
        {
            int rAvg = 0, gAvg = 0, bAvg = 0;

            for (int i = x; i < x + sizeX; i++)
            {
                for (int j = y; j < y + sizeY; j++)
                {
                    Color color = bitmap.GetPixel(i, j);
                    rAvg += color.R;
                    gAvg += color.G;
                    bAvg += color.B;
                }
            }

            int sizeSq = sizeX * sizeY;

            rAvg /= sizeSq;
            gAvg /= sizeSq;
            bAvg /= sizeSq;

            return Color.FromArgb(rAvg, gAvg, bAvg);
        }

        private static string GetAnswer(string question, string[] answers = null)
        {
            if (answers != null)
            {
                bool correctAnswer = false;
                int answer = 0;
                while (!correctAnswer)
                {
                    Console.WriteLine(question);
                    Console.WriteLine("Wpisz znak odpowiadający danej odpowiedzi i zatwierdź enterem.");
                    for (int i = 0; i < answers.Length; i++) Console.WriteLine($"{i + 1}. {answers[i]}");
                    string response = Console.ReadLine();

                    try
                    {
                        answer = int.Parse(response);
                        if (answer < 1 || answer > answers.Length) Console.WriteLine("Nieprawidłowa liczba\n");
                        else correctAnswer = true;
                    }
                    catch
                    {
                        Console.WriteLine("Nie podano liczby\n");
                    }
                }
                Console.WriteLine();
                return answer.ToString();
            }
            else
            {
                Console.WriteLine(question);
                string response = Console.ReadLine();
                Console.WriteLine();
                return response;
            }
        }

        private static int GetAnswerInt(string question, int min = int.MinValue, int max = int.MaxValue)
        {
            bool correctAnswer = false;
            int answer = 0;
            while (!correctAnswer)
            {
                Console.WriteLine(question);
                Console.WriteLine($"Wpisz liczbę całkowitą między {min} a {max} (włącznie).");
                string response = Console.ReadLine();

                try
                {
                    answer = int.Parse(response);
                    if (answer < min || answer > max) Console.WriteLine("Nieprawidłowa liczba\n");
                    else correctAnswer = true;
                }
                catch
                {
                    Console.WriteLine("Nie podano liczby\n");
                }
            }
            Console.WriteLine();
            return answer;
        }
    }
}
