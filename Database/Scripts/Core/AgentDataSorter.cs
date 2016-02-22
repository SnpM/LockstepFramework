using Lockstep.Data;
using Lockstep;
public static class AgentDataSorter {
    static int OrderUnitsFirst (DataItem item) {
        return SortByType (item, AgentType.Unit);
    }
    static int OrderBuildingsFirst (DataItem item) {
        return SortByType (item, AgentType.Building);
    }
    static int SortByType (DataItem item, AgentType agentType) {
        AgentDataItem agentInterfacer = item as AgentDataItem;
        if (agentInterfacer == null) return -1;
        return agentInterfacer.SortDegreeFromAgentType(agentType);
    }

}