using System;
using System.Collections.Generic;
using System.IO;

namespace Bot_Application1.Dialogs
{
    public class DialogQA
    {

        List<QA> qaList = new List<QA>();

        public DialogQA()
        {
            load();
        }

        public void load()
        {
            StreamReader Q = File.OpenText(@"F:\Visual Studio 2017\Projects\Bot Application1\Bot Application1\bin\Dialogs\QA.txt");
            string kb = Q.ReadToEnd();
            string[] blocks = kb.Split("Q:".ToCharArray());
            for (int i = 0; i < blocks.Length; i++)
            {
                string block = blocks[i].Trim();
                if (block.Length == 0) continue;
                QA qa = new QA(block);
                qaList.Add(qa);
            }
        }

        public string answer(string input)
        {
            for (int i = 0; i < qaList.Count; i++)
            {
                QA qa = qaList[i];
                string answer = qa.answer(input);
                if (answer != null)
                    return answer;
            }
            return "然後呢?";
        }

        class QA
        {
            static Random random = new Random(3767);
            string[] q, a;

            public  QA(string block)
            {
                block = block.Replace("\r", "");
                block = block.Replace(" ", "");
                //STR.replace(block, "\r", "");
               // block = STR.replace(block, " ", "");
                string head = STR.head(block, "\n");
                string tail = STR.tail(block, "\n");

                q = head.Split('|');
                a = tail.Split('\n');
            }
            public string answer(string input)
            {
                for (int i = 0; i < q.Length; i++)
                {
                    int matchAt = input.IndexOf(q[i]);
                    string tail;
                    if (matchAt >= 0)
                        tail = input.Substring(matchAt + q[i].Length);
                    else if (q[i].Equals("比對失敗"))
                        tail = "";
                    else
                        continue;
                    int aIdx = Math.Abs(new Random().Next()) % a.Length;
                    string answer = STR.replace(a[aIdx], "*", tail);
                    answer = STR.expand(answer, "我=");
                    return answer;
                }
                return null;
            }
        }

        class STR
        {
            public static string head(string pStr, string pSpliter)
            {
                int spliterPos = pStr.IndexOf(pSpliter);
                if (spliterPos < 0) return pStr;
                return pStr.Substring(0, spliterPos);
            }

            public static string tail(string pStr, string pSpliter)
            {
                int spliterPos = pStr.IndexOf(pSpliter);
                if (spliterPos < 0) return "";
                return pStr.Substring(spliterPos + pSpliter.Length);
            }

            public static string replace(string pStr, string fromPat, string toPat)
            {
                if (fromPat.Length == 0) return pStr;
                if (pStr.IndexOf(fromPat) < 0) return pStr;
                string rzStr = "";
                int strIdx = 0, nextIdx;
                while ((nextIdx = pStr.IndexOf(fromPat, strIdx)) >= 0)
                {
                    rzStr += (pStr.Substring(strIdx, nextIdx));
                    rzStr += (toPat);
                    strIdx = nextIdx + fromPat.Length;
                }
                rzStr += (pStr.Substring(strIdx));
                return rzStr;
            }

            public static string expand(string pText, string pMacros)
            {
                string[] macros = pMacros.Split('|');
                for (int i = 0; i < macros.Length; i++)
                {
                    string name = head(macros[i], "=");
                    string expand = tail(macros[i], "=");
                    pText = replace(pText, name, expand);
                }
                return pText;
            }
        }
    }
}