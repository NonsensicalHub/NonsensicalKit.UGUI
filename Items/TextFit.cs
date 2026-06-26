using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace NonsensicalKit.UGUI
{
    /// <summary>
    /// 文本自动换行，使得标点符号不会显示在行首
    /// </summary>
    public class TextFit : Text
    {
        /// <summary>
        /// 用于匹配标点符号（正则表达式）
        /// </summary>
        private const string REGEX_1 = @"\p{P}";

        private const string REGEX_2 = "^['\"{}\\(\\)\\[\\]\\*&.?!,…:;，。、‘’“”；]+$";

        /// <summary>
        /// 用于存储text组件中的内容
        /// </summary>
        private StringBuilder _MExplainText = null;

        /// <summary>
        /// 用于存储text生成器中的内容
        /// </summary>
        private IList<UILineInfo> _MExpalinTextLine;

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            base.OnPopulateMesh(toFill);
            StartCoroutine(MClearUpExplainMode(this, text));
        }

        /// <summary>
        /// 整理文字。确保首字母不出现标点
        /// </summary>
        /// <param name="component">text组件</param>
        /// <param name="txt">需要填入text中的内容</param>
        /// <returns></returns>
        private IEnumerator MClearUpExplainMode(Text component, string txt)
        {
            component.text = txt;

            //如果直接执行下边方法的话，那么_component.cachedTextGenerator.lines将会获取的是之前text中的内容，而不是_text的内容，所以需要等待一下
            yield return null;

            _MExpalinTextLine = component.cachedTextGenerator.lines;

            //需要改变的字符序号

            _MExplainText = new StringBuilder(component.text);

            for (int i = 1; i < _MExpalinTextLine.Count; i++)
            {
                //首位是否有标点
                bool b = Regex.IsMatch(component.text[_MExpalinTextLine[i].startCharIdx].ToString(), REGEX_1);

                if (b)
                {
                    var mChangeIndex = _MExpalinTextLine[i].startCharIdx - 1;

                    _MExplainText.Insert(mChangeIndex, "\n");
                }
            }

            component.text = _MExplainText.ToString();

            //_component.txt = txt;
        }

        /// <summary>
        /// 整理文字。确保首字母不出现标点
        /// </summary>
        /// <param name="component">text组件</param>
        /// <param name="txt">需要填入text中的内容</param>
        /// <returns></returns>
        private IEnumerator MClearUpExplainMode2(Text component, string txt)
        {
            component.text = txt;
            bool flag = true;
            while (flag)
            {
                flag = false;

                //如果直接执行下边方法的话，那么_component.cachedTextGenerator.lines将会获取的是之前text中的内容，而不是_text的内容，所以需要等待一下
                yield return null;

                var explainTextLine = component.cachedTextGenerator.lines;

                //需要改变的字符序号
                var explainText = new StringBuilder(component.text);

                for (int i = 1; i < explainTextLine.Count; i++)
                {
                    if (explainTextLine[i].startCharIdx < component.text.Length)
                    {
                        //首位是否有标点
                        bool b = Regex.IsMatch(component.text[explainTextLine[i].startCharIdx].ToString(), REGEX_2);

                        if (b && explainTextLine[i].startCharIdx > 0)
                        {
                            bool bb = Regex.IsMatch(component.text[explainTextLine[i].startCharIdx - 1].ToString(), REGEX_2);
                            if (!bb)
                            {
                                flag = true;
                                var mChangeIndex = explainTextLine[i].startCharIdx - 1;

                                explainText.Insert(mChangeIndex, "\n");
                            }
                        }
                    }
                }

                component.text = explainText.ToString();
            }
        }
    }
}
