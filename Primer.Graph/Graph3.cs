﻿using System;
using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using UnityEngine;

namespace Primer.Graph
{
    public class Graph3 : MonoBehaviour, IDisposable
    {
        public bool enableZAxis;
        public bool isRightHanded = true;

        public Axis3 xAxis => transform.Find("X").GetComponent<Axis3>(); 
        public Axis3 yAxis => transform.Find("Y").GetComponent<Axis3>(); 
        public Axis3 zAxis => transform.Find("Z").GetComponent<Axis3>();
        private List<Axis3> axes => new() { xAxis, yAxis, zAxis };

        public Tween Transition()
        {
            var removeTransitions = new List<Tween>();
            var updateTransitions = new List<Tween>();
            var addTransitions = new List<Tween>();

            foreach (var axis in axes)
            {
                if (axis.length == 0) continue;
                var (remove, update, add) = axis.PrepareTransitionParts();
                removeTransitions.Add(remove);
                updateTransitions.Add(update);
                addTransitions.Add(add);
            }
            updateTransitions.AddRange(
                GetComponentsInChildren<IPrimerGraphData>().Select(x => x.Transition())
            );
            return Tween.Series(
                removeTransitions.RunInParallel(),
                updateTransitions.RunInParallel(),
                addTransitions.RunInParallel()
            );
        }
        // We assume there will be no data present when the axes are drown, so they appear independently
        public Tween Appear() => axes.Select(x => x.Appear()).RunInParallel();
        public Tween Disappear() => axes.Select(x => x.Disappear()).RunInParallel();
        
        public void Dispose()
        {
            gameObject.SetActive(false);
        }
        
        public PrimerLine AddLine(string name)
        {
            var gnome = new Primer.SimpleGnome(transform);
            var line = gnome.Add<PrimerLine>(name);
            line.transformPointFromDataSpaceToPositionSpace = DataSpaceToPositionSpace;
            return line;
        }
        
        public StackedArea AddStackedArea(string name)
        {
            var gnome = new Primer.SimpleGnome(transform);
            var area = gnome.Add<StackedArea>(name);
            area.transformPointFromDataSpaceToPositionSpace = DataSpaceToPositionSpace;
            return area;
        }
        
        public BarPlot AddBarPlot(string name)
        {
            var gnome = new Primer.SimpleGnome(transform);
            var barPlot = gnome.Add<BarPlot>(name);
            barPlot.transformPointFromDataSpaceToPositionSpace = DataSpaceToPositionSpace;
            return barPlot;
        }
        
        public Vector3 DataSpaceToPositionSpace(Vector3 point)
        {
            return new Vector3(
                (point.x - xAxis.min) / xAxis.rangeSize * xAxis.lengthMinusPadding,
                (point.y - yAxis.min) / yAxis.rangeSize * yAxis.lengthMinusPadding,
                (point.z - zAxis.min) / zAxis.rangeSize * zAxis.lengthMinusPadding
            );
        }
    }
    
    public interface IPrimerGraphData
    {
        public Tween Transition();
    }
}