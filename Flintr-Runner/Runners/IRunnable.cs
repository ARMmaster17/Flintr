using Flintr_Runner.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Flintr_Runner.Runners
{
    internal interface IRunnable
    {
        void Setup(RuntimeConfiguration runtimeConfiguration);

        void Run();

        void Kill();
    }
}