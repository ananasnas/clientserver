using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using library;
using System.Runtime.Serialization;
using System.Configuration;
using System.Threading;

namespace klient
{
    class Program
    {
        public static byte[] Serialization(Product obj)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, obj);
            byte[] msg = stream.ToArray();
            return msg;
        }

        public static Product DeSerialization(byte[] serializedAsBytes)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            stream.Write(serializedAsBytes, 0, serializedAsBytes.Length);
            stream.Seek(0, SeekOrigin.Begin);
            return (Product)formatter.Deserialize(stream);
        }
        public static object Lock = new object();
        static void Main(string[] args)
        {
            for (int i = 0; i < 10; i++)
            {
                Thread t1 = new Thread(new ParameterizedThreadStart(Start));
                Random r = new Random();
                Thread.Sleep(r.Next(500, 1500));
                t1.Start(i);
            }     
        }
        public static Object Lock1 = new Object();
        public static void Start(object num)
        {
            int num_ = (int)num;
             TcpClient client = new TcpClient("127.0.0.1", 8888); 
       
            while (true)
            {
                lock (Lock1)
                {
                    NetworkStream stream = client.GetStream();
                    Product product = new Product();

                    Random rand = new Random();
                    int temp;
                    temp = rand.Next(1, 4);
                    int type_of_trans_ = temp;

                    byte[] buf = new byte[1024];
                    product.type_of_trans = type_of_trans_;
                    product.ClientNumber = num_;
                    string Chars = "knisfisfcskdn";

                    product.name = Chars[rand.Next(0, Chars.Length)].ToString();

                    if (type_of_trans_ == 2)
                    {
                        product.price = rand.Next(1, 100);
                    }
                    buf = Serialization(product);

                    stream.Write(buf, 0, buf.Length);
            
                    buf = new byte[1024];

                    int bytes = 0;

                    Console.WriteLine("Клиент номер " + num_ + ":");
                  
                        Product p = new Product();
                        do
                        {
                          
                            bytes = stream.Read(buf, 0, buf.Length);
                            p = DeSerialization(buf);
                        }
                        while (stream.DataAvailable);
                           
                        if (p.type_of_trans == 3)
                            {
                                if (p.name !=null)
                                {
                                    lock (Lock)
                                    {
                                        Console.WriteLine("Найден элемент:");
                                        Console.WriteLine(p.name + "," + p.price.ToString() + "," + p.type_of_trans.ToString());
                                    }
                                }
                                else
                                {
                                    lock (Lock)
                                        { Console.WriteLine("Ошибка! Элемент не найден.\n"); }
                                }
                            }
                        else
                        {
                        lock (Lock)
                        {
                            Console.WriteLine(p.name);
                        }
                        }
                        Random r = new Random();
                        Thread.Sleep(r.Next(500, 1500));
                    
                }
            }
            
        }
    }
}


