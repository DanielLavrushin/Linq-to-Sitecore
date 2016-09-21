using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;

namespace LinqToSitecore.VisualStudio
{
 public   class LinqToSitecoreFileGenerator: IVsSingleFileGenerator
    {
     public int DefaultExtension(out string pbstrDefaultExtension)
     {
         throw new NotImplementedException();
     }

     public int Generate(string wszInputFilePath, string bstrInputFileContents, string wszDefaultNamespace,
         IntPtr[] rgbOutputFileContents, out uint pcbOutput, IVsGeneratorProgress pGenerateProgress)
     {
         throw new NotImplementedException();
     }
    }
}
