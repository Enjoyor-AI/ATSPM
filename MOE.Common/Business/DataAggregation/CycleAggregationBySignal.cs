﻿using System;
using System.Collections.Generic;
using System.Linq;
using MOE.Common.Business.Bins;
using MOE.Common.Business.WCFServiceLibrary;
using MOE.Common.Models;

namespace MOE.Common.Business.DataAggregation
{
    public class CycleAggregationBySignal : AggregationBySignal
    {
        public List<CycleAggregationByApproach> ApproachCycles { get; }

        public CycleAggregationBySignal(ApproachCycleAggregationOptions options, Models.Signal signal) : base(
            options, signal)
        {
            ApproachCycles = new List<CycleAggregationByApproach>();
            GetApproachCycleAggregationContainersForAllApporaches(options, signal);
            LoadBins(null, null);
        }


        public CycleAggregationBySignal(ApproachCycleAggregationOptions options, Models.Signal signal,
            int phaseNumber) : base(options, signal)
        {
            ApproachCycles = new List<CycleAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.ProtectedPhaseNumber == phaseNumber)
                {
                    ApproachCycles.Add(
                        new CycleAggregationByApproach(approach, options, options.StartDate,
                            options.EndDate,
                            true, options.SelectedAggregatedDataType));
                    if (approach.PermissivePhaseNumber != null && approach.PermissivePhaseNumber == phaseNumber)
                        ApproachCycles.Add(
                            new CycleAggregationByApproach(approach, options, options.StartDate,
                                options.EndDate,
                                false, options.SelectedAggregatedDataType));
                }
            LoadBins(null, null);
        }

        public CycleAggregationBySignal(ApproachCycleAggregationOptions options, Models.Signal signal,
            DirectionType direction) : base(options, signal)
        {
            ApproachCycles = new List<CycleAggregationByApproach>();
            foreach (var approach in signal.Approaches)
                if (approach.DirectionType.DirectionTypeID == direction.DirectionTypeID)
                {
                    ApproachCycles.Add(
                        new CycleAggregationByApproach(approach, options, options.StartDate,
                            options.EndDate,
                            true, options.SelectedAggregatedDataType));
                    if (approach.PermissivePhaseNumber != null)
                        ApproachCycles.Add(
                            new CycleAggregationByApproach(approach, options, options.StartDate,
                                options.EndDate,
                                false, options.SelectedAggregatedDataType));
                }
            LoadBins(null, null);
        }

        protected override void LoadBins(SignalAggregationMetricOptions options, Models.Signal signal)
        {
            for (var i = 0; i < BinsContainers.Count; i++)
            for (var binIndex = 0; binIndex < BinsContainers[i].Bins.Count; binIndex++)
            {
                var bin = BinsContainers[i].Bins[binIndex];
                foreach (var approachCycleAggregationContainer in ApproachCycles)
                {
                    bin.Sum += approachCycleAggregationContainer.BinsContainers[i].Bins[binIndex].Sum;
                    bin.Average = ApproachCycles.Count > 0 ? bin.Sum / ApproachCycles.Count : 0;
                }

            }
        }

        protected override void LoadBins(ApproachAggregationMetricOptions options, Models.Signal signal)
        {
            throw new NotImplementedException();
        }


        private void GetApproachCycleAggregationContainersForAllApporaches(
            ApproachCycleAggregationOptions options, Models.Signal signal)
        {
            foreach (var approach in signal.Approaches)
            {
                ApproachCycles.Add(
                    new CycleAggregationByApproach(approach, options, options.StartDate,
                        options.EndDate,
                        true, options.SelectedAggregatedDataType));
                if (approach.PermissivePhaseNumber != null)
                    ApproachCycles.Add(
                        new CycleAggregationByApproach(approach, options, options.StartDate,
                            options.EndDate,
                            false, options.SelectedAggregatedDataType));
            }
        }


        public int GetCyclesByDirection(DirectionType direction)
        {
            var splitFails = 0;
            if (ApproachCycles != null)
                splitFails = ApproachCycles
                    .Where(a => a.Approach.DirectionType.DirectionTypeID == direction.DirectionTypeID)
                    .Sum(a => a.BinsContainers.FirstOrDefault().SumValue);
            return splitFails;
        }

        public int GetAverageCyclesByDirection(DirectionType direction)
        {
            var approachCyclesByDirection = ApproachCycles
                .Where(a => a.Approach.DirectionType.DirectionTypeID == direction.DirectionTypeID);
            var splitFails = 0;
            if (approachCyclesByDirection.Any())
                splitFails = Convert.ToInt32(Math.Round(approachCyclesByDirection
                    .Average(a => a.BinsContainers.FirstOrDefault().SumValue)));
            return splitFails;
        }
    }
}