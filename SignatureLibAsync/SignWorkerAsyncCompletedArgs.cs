using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignatureLib;
using SignatureWorkerAbstracts;

namespace SignatureLibAsync
{
    public class SignWorkerAsyncCompletedArgs : SignWorkerCompletedAbstractArgs
    {
        public SignWorkerAsyncCompletedArgs(TimeSpan timeProcessing, Dictionary<string, int> stat, Exception errorSignWorker, Exception errorTaskQueue)
        {
            ErrorTaskQueue = errorTaskQueue;
            ErrorSignWorker = errorSignWorker;
            _stat = stat;
            TimeProcessing = timeProcessing;
        }

        private Dictionary<string, int> _stat { get; }

        public override long CountBlocks
        {
            get
            {
                if (_stat.ContainsKey(nameof(TaskCalcHashSha256)))
                    return _stat[nameof(TaskCalcHashSha256)];
                return 0;
            }
        }
    }
}
