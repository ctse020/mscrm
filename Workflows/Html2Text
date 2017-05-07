using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Activities;
using System.Text.RegularExpressions;

using HtmlAgilityPack;
using Microsoft.Xrm.Sdk.Workflow;

namespace Ctse.MSCRM.Workflows
{
    public class Html2Text : CodeActivity
    {

        [RequiredArgument]
        [Input("InputHtml")]
        public InArgument<string> InputHtml { get; set; }

        [Output("OutputText")]
        public OutArgument<string> OutputText { get; set; }

        [Output("Execution summary")]
        public OutArgument<string> ExecutionSummary { get; set; }

        [Output("Errors occurred")]
        public OutArgument<bool> ErrorsOccurred { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            string errMsg = "";
            try
            {
                string _html = this.InputHtml.Get(executionContext);
                string _txt = ExtractHtmlInnerText(_html);
                this.OutputText.Set(executionContext, _txt);
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
                if (!string.IsNullOrWhiteSpace(ex.Source))
                    errMsg = errMsg + "\r\n\r\n" + ex.Source;
                if (ex.InnerException != null)
                {
                    errMsg = errMsg + "\r\n\r\n" + ex.InnerException.Message;
                    if (!string.IsNullOrWhiteSpace(ex.InnerException.Source))
                        errMsg = errMsg + "\r\n\r\n" + ex.InnerException.Source;
                    if (!string.IsNullOrWhiteSpace(ex.InnerException.StackTrace))
                        errMsg = errMsg + "\r\n\r\n" + ex.InnerException.StackTrace;
                }
                errMsg = errMsg + "\r\n\r\n" + ex.StackTrace;
            }
            finally
            {
                executionContext.SetValue<string>(this.ExecutionSummary, errMsg);
                executionContext.SetValue<bool>(this.ErrorsOccurred, !string.IsNullOrWhiteSpace(errMsg));
            }
        }

        private string ExtractHtmlInnerText(string html)
        {
            string txt = "";
            if (!string.IsNullOrWhiteSpace(html))
            { 
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(html);
                var body = doc.DocumentNode.SelectSingleNode("//body");
                if (body == null)
                {
                    html = "<html><body>" + html + "</body></html>";
                    doc.LoadHtml(html);
                    body = doc.DocumentNode.SelectSingleNode("//body");
                }

                var nodes = body.SelectNodes("//br");
                if (nodes != null)
                    foreach (var node in nodes)
                        node.ParentNode.ReplaceChild(doc.CreateTextNode("\r\n"), node);

                txt = body.InnerText;
                
                txt = System.Net.WebUtility.HtmlDecode(txt);

                Regex regex = new Regex("(<.*?>\\s*)+", System.Text.RegularExpressions.RegexOptions.Singleline);
                txt = regex.Replace(txt, " ").Trim();
                txt = txt.Replace("\r\n\r\n\r\n", "\r\n\r\n");

                doc = null;
            }
            return txt;
        }
    }
}
