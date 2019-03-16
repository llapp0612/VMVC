using System;

public class Class1
{
	public Class1()
	{
        void Tick()
        {
            int counter = 0;
            string line;
            int milliseconds = 2000;

            while (true)
            {
                System.IO.StreamReader file = new System.IO.StreamReader(@"C:\Users\Rene\Desktop\VMVC\state.txt");
                while ((line = file.ReadLine()) != null)
                {
                    counter++;
                }

                file.Close();
                MessageBox.Show(line);
                Thread.Sleep(milliseconds);
            }
        }

        void Main(string[] args)
        {
            var class1 = new Class1();
            class1.Tick();
        }
    }
}
