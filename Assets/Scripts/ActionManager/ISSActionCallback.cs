using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public enum SSActionEventType:int { Started, Competeted }

public interface ISSActionCallback {
    void SSActionEvent(SSAction source,
        SSActionEventType events = SSActionEventType.Competeted,
        int intParam = 0,
        string strParam = null,
        Object objectParam = null);
}


