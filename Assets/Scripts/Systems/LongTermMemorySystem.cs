using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct LongTermMemorySystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new(Allocator.TempJob);

        // Find NPCs with new Long Term Memory formation due
        foreach (var (npc, memory) in SystemAPI.Query<RefRO<NPC>, RefRW<ShortTermMemory>>())
        {
            memory.ValueRW.TimeSinceLastInterval += SystemAPI.Time.DeltaTime;
            if (memory.ValueRO.TimeSinceLastInterval < memory.ValueRO.TimeInterval)
                continue;

            if (memory.ValueRO.QueueLongTermMemory)
                memory.ValueRW.QueueLongTermMemory = true;

            // Reset memory interval timer
            memory.ValueRW.TimeSinceLastInterval = 0.0f;
        }

        // Process NPCs
        foreach (var (npc, memory, shortMemoryBuffer, longMemoryBuffer, longMemoryPeriodBuffer, npcEntity)
            in SystemAPI.Query<RefRO<NPC>, RefRW<ShortTermMemory>, DynamicBuffer<ShortTermMemoryBuffer>,
            DynamicBuffer<LongTermMemoryBuffer>, DynamicBuffer <LongTermMemoryPeriod>>()
            .WithEntityAccess())
        {
            if (!memory.ValueRO.QueueLongTermMemory)
                continue;

            // Create long term memory
            var shortMemoryArray = shortMemoryBuffer.AsNativeArray();

            // Sort short term memories by emotion intensity
            // Keep only top memory of each Type present
            SortJob<ShortTermMemoryBuffer, CustomComparer> sortJob = shortMemoryArray.SortJob<ShortTermMemoryBuffer, CustomComparer>(default);
            var jobHandle = sortJob.Schedule();
            jobHandle.Complete();
            //longMemoryArray.GroupBy(m => m.Memory.Type).Select(g => g.First());

            // Take top MemoryLimit num. of short term memories into long term memory
            int longTermLength = 0;
            for (int i = 0; i < memory.ValueRO.MemoryLimit; i++)
            {
                if (i > shortMemoryArray.Length)
                    break;

                LongTermMemoryBuffer copyMemory = new() { Memory = shortMemoryArray[i].Memory };
                ecb.AppendToBuffer(npcEntity, copyMemory);
                longTermLength++;
            }

            // Calculate and store Mood

            // Store position of long term memory period in the flattened memory buffer
            LongTermMemoryPeriod longTermMemoryPeriod = new()
            {
                LongTermMemoryIndex = longMemoryBuffer.Length,
                LongTermMemoryCount = longTermLength,
            };
            ecb.AppendToBuffer(npcEntity, longTermMemoryPeriod);

            // Clear short term memory
            ecb.AddBuffer<ShortTermMemoryBuffer>(npcEntity).Clear();

            memory.ValueRW.QueueLongTermMemory = false;
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}

struct CustomComparer : IComparer<ShortTermMemoryBuffer>
{
    public int Compare(ShortTermMemoryBuffer x, ShortTermMemoryBuffer y)
    {
        if (x.Memory.EmotionResponse[0] < y.Memory.EmotionResponse[0])
            return -1;

        if (x.Memory.EmotionResponse[0] > y.Memory.EmotionResponse[0])
            return 1;

        return 0;
    }
}