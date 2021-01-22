using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TP_Lab1
{
    class Program
    {
        public static readonly string Path = "tableSheet.txt";
        static void Main(string[] args)
        {
            string GenerateTable()
            {
                string GenerateName(Random rnd)
                {
                    string гласные = "аеёийоуыэюя";
                    string согласные = "бвгджзклмнпрстфхцчшщ";
                    string name = string.Empty;//((char)rnd.Next('А', 'Я')).ToString();

                    for (int i = 0; i < rnd.Next(2, 8); i++)
                        if (i % 2 == 0)
                            name += гласные[rnd.Next(гласные.Length)];
                        else
                            name += согласные[rnd.Next(согласные.Length)];

                    return name.ToUpper()[0] + name.Substring(1);
                }

                List<TableNode> табличка = new List<TableNode>();
                Random rand = new Random();

                List<string> названияПредметов = new List<string>();
                for (int i = 0; i < rand.Next(3,5); i++)
                    названияПредметов.Add(GenerateName(rand));

                //строки таблицы
                for (int i = 0; i < rand.Next(5,20); i++)
                {
                    var запись = new TableNode
                    {
                        Возраст = rand.Next(7, 18), 
                        Фамилия = GenerateName(rand),
                        Пол = typeof(TableNode.ВариантыПолов).GetEnumValues().Cast<TableNode.ВариантыПолов>().ToList()[rand.Next(typeof(TableNode.ВариантыПолов).GetEnumValues().Length)],
                        Предметы = названияПредметов.Select(t=>new TableNode.Предмет{ Название = t,Оценка = rand.Next(2,6)}).ToList()
                    };

                    табличка.Add(запись);
                }

                return string.Join("\n",табличка);
            }

            string сгенерированнаяТабличка = GenerateTable();

            using (var sw = new StreamWriter(Path, false))
                sw.Write(сгенерированнаяТабличка);

            ////////////////////////////////////////////////////////////////
            
            var загруженнаяТабличка = new List<TableNode>();
            using (var sr = new StreamReader(Path))
            {
                while (!sr.EndOfStream)
                {
                    var записьСырая = sr.ReadLine();
                    if (записьСырая != null)
                    {
                        var записьСписком = new Queue<string>(записьСырая.Split(';').Select(t => t.Trim()));

                        var запись = new TableNode
                        {
                            Фамилия = записьСписком.Dequeue(),
                            Возраст = int.Parse(записьСписком.Dequeue()),
                            Пол = (TableNode.ВариантыПолов) Enum.Parse(typeof(TableNode.ВариантыПолов),
                                записьСписком.Dequeue())
                        };

                        while (записьСписком.Any())
                        {
                            var предметСырой = new Queue<string>(записьСписком.Dequeue().Split(':'));

                            запись.Предметы.Add(new TableNode.Предмет
                            {
                                Название = предметСырой.Dequeue(),
                                Оценка = int.Parse(предметСырой.Dequeue())
                            });
                        }

                        загруженнаяТабличка.Add(запись);
                    }
                }
            }

            Console.WriteLine(string.Join("\n",загруженнаяТабличка)+"\n");

            Console.WriteLine($"Средний возраст учеников: {загруженнаяТабличка.Select(t=>t.Возраст).Sum()/загруженнаяТабличка.Count}");

            Console.WriteLine($"Студенты-отличники: {string.Join(" ", загруженнаяТабличка.Where(t => t.Предметы.All(p => p.Оценка == 5)).Select(t => t.Фамилия))}");

            Console.WriteLine($"Количество мужчин: {загруженнаяТабличка.Count(t => t.Пол == TableNode.ВариантыПолов.Мужской)}\nколичество женщин: {загруженнаяТабличка.Count(t => t.Пол == TableNode.ВариантыПолов.Женский)}");

            Console.WriteLine($"Средние баллы: {string.Join("; ", загруженнаяТабличка.Select(t => $"{t.Фамилия} {t.Предметы.Sum(предмет => предмет.Оценка) / t.Предметы.Count}"))}");

            Console.WriteLine($"Средний балл по {string.Join("; ", загруженнаяТабличка.SelectMany(t => t.Предметы).GroupBy(t => t.Название).Select(t => $"{t.Key} - {t.Sum(предмет => предмет.Оценка) / (float)t.Count():f2}"))}");

            Console.WriteLine($"Статистика оценок: \n{string.Join("\n", загруженнаяТабличка.SelectMany(t => t.Предметы).GroupBy(t => t.Название).Select(t=>$"{t.Key} {string.Join(", ",t.GroupBy(p => p.Оценка).OrderByDescending(p=>p.Key).Select(p => $"Оценок {p.Key} - {p.Count()}шт."))}"))}");

            Console.ReadKey();
        }
    }

    class TableNode
    {
        public string Фамилия { get; set; }
        public int Возраст { get; set; }
        public ВариантыПолов Пол { get; set; }
        public List<Предмет> Предметы { get; set; } = new List<Предмет>();

        public enum ВариантыПолов
        {
            Мужской,
            Женский,
            Не_указан
        }

        public override string ToString()
        {
            return string.Join("; ",typeof(TableNode).GetProperties().Where(t=>t.PropertyType!= typeof(List<Предмет>)).Select(t => t.GetValue(this)).Concat(Предметы.Select(t=>string.Join(":", typeof(Предмет).GetProperties().Select(s=>s.GetValue(t))))));
        }

        public class Предмет
        {
            public string Название { get; set; }
            public int Оценка { get; set; }
        }
    }
}
