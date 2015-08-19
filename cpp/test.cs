static void Main(string[] args)
        {
            test("iajsdğiofjasĐdkjfasŸ48596đ060= =8-8'Ç;");
        }

        static void test(string s)
        {
            foreach(char c in s)
            {
                if(c >= 0 && c <= 127)
                {
                    Console.WriteLine("ascii char:{0} {1}", c, (int)c);
                }
                else
                {
                    Console.WriteLine("unicode char:{0} {1}", c, (int)c);
                }
            }
        }