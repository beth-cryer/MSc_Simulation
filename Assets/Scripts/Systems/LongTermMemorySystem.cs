using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial struct LongTermMemorySystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new(Allocator.TempJob);

        // Find NPCs with new Long Term Memory formation due
        foreach (var memory in SystemAPI.Query<RefRW<ShortTermMemory>>()
		.WithAll<NPC>())
        {
            memory.ValueRW.TimeSinceLastInterval += SystemAPI.Time.DeltaTime;
            if (memory.ValueRO.TimeSinceLastInterval < memory.ValueRO.TimeInterval)
                continue;

            if (!memory.ValueRO.QueueLongTermMemory)
                memory.ValueRW.QueueLongTermMemory = true;

            // Reset memory interval timer
            memory.ValueRW.TimeSinceLastInterval = 0.0f;
        }

        // Process NPCs
        foreach (var (memory, shortMemoryBuffer, longMemoryBuffer, npcEntity)
            in SystemAPI.Query<RefRW<ShortTermMemory>, DynamicBuffer<ShortTermMemoryBuffer>,
            DynamicBuffer<LongTermMemoryBuffer>>()
			.WithPresent<NPC>()
            .WithEntityAccess())
        {
            if (!memory.ValueRO.QueueLongTermMemory)
                continue;

            // Create long term memory
			/*
            NativeArray<ShortTermMemoryBuffer> shortMemoryArray = shortMemoryBuffer.AsNativeArray();

            // Sort short term memories by emotion intensity
            SortJob<ShortTermMemoryBuffer, CustomComparer> sortJob = shortMemoryArray.SortJob<ShortTermMemoryBuffer, CustomComparer>(default);
            var jobHandle = sortJob.Schedule();
            jobHandle.Complete();

            // Filter out memories of the same type, keeping only the most intense memory of each type
            NativeArray<ShortTermMemoryBuffer> memoryFilteredTypes = new(shortMemoryArray.Length, Allocator.TempJob);
            for (int t = 0; t < (int)EEmotion.COUNT; t++)
            {
                int bestMemoryOfType = -1;
                for (int i = 0; i < shortMemoryArray.Length; i++)
                {
                    if ((int)shortMemoryArray[i].Memory.Type != t)
                        continue;

                    // First memory of type is automatically the best
                    if (bestMemoryOfType == -1)
                    {
                        bestMemoryOfType = i;
                        continue;
                    }

                    // Replace best memory if we find a more intense one here
                    if (Compare(shortMemoryArray[i], shortMemoryArray[bestMemoryOfType]) > 0)
                        bestMemoryOfType = i;
                }

                if (bestMemoryOfType == -1)
                    continue;

                memoryFilteredTypes[memoryFilteredTypes.Length-1] = shortMemoryArray[bestMemoryOfType];
            }
			*/

			//longMemoryArray.GroupBy(m => m.Memory.Type).Select(g => g.First());
			// (could have been this 1 line but alas....burstcompiler my nemesis......)

			// Take top MemoryLimit num. of short term memories into long term memory
			int longTermLength = 0;
            for (int i = 0; i < memory.ValueRO.MemoryLimit; i++)
            {
                if (i >= shortMemoryBuffer.Length)
                    break;

                LongTermMemoryBuffer copyMemory = new() { Memory = shortMemoryBuffer[i].Memory };
                ecb.AppendToBuffer(npcEntity, copyMemory);
                longTermLength++;
            }

			// Calculate and store Mood
			//
			double timeElapsedCurrent = SystemAPI.Time.ElapsedTime;
			float runningWeightedLength = 0f;
			float3 runningTotal = 0f;
			foreach (var shortMemory in shortMemoryBuffer)
			{
				float timeElapsed = (float)math.clamp(timeElapsedCurrent - shortMemory.Memory.TimeElapsed, 0f, 60f);
				runningWeightedLength += 1 - (timeElapsed / 60.0f);
				runningTotal += shortMemory.Memory.EmotionResponse;
			}
			float3 mood = (runningWeightedLength > 0f) ?
				runningTotal / runningWeightedLength
				: new(0.0f, 0.0f, 0.0f);
			// (TODO: make this not copy-pasted from MoodCalculationSystem, ideally)

			// Store position of long term memory period in the flattened memory buffer
			LongTermMemoryPeriod longTermMemoryPeriod = new()
            {
				MemoryPeriodMood = mood,
				MemoryTimeElapsed = SystemAPI.Time.ElapsedTime,
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

    [BurstCompile]
    public int Compare(ShortTermMemoryBuffer x, ShortTermMemoryBuffer y)
    {
		float memValueX = math.abs(x.Memory.EmotionResponse[0]) + math.abs(x.Memory.EmotionResponse[1]) + math.abs(x.Memory.EmotionResponse[2]);
		float memValueY = math.abs(y.Memory.EmotionResponse[0]) + math.abs(y.Memory.EmotionResponse[1]) + math.abs(y.Memory.EmotionResponse[2]);

		if (memValueX < memValueY)
			return -1;

        if (memValueX > memValueY)
            return 1;

        return 0;
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