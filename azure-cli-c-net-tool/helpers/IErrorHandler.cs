using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cli_repo_program.helpers
{
    /**
     * Created 2.15.23 AG
     **/
    internal interface IErrorHandler
    {
        /**
         * Takes in an error and displays appropriately. 
         **/
        void HandleError(string error);
    }
}
