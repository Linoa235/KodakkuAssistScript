using System;
using System.Numerics;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
using System.Windows.Forms;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;

namespace MyScriptNamespace
{
    [ScriptType(name: "FRU P5 Floor Fire Grid Guidance", territorys: [], $16adc08ae-bdf8-45f3-a110-be32b256c178", version: "0.0.0.1", Author: "Linoa235", guid: "767a47d6-2adc-4583-9f4d-e1e29d7a6110")]
    public class FRUScript
    {
        [UserSetting("Floor Fire Safe Guidance Color")]
        public ScriptColor BladeSafeColor { get; set; } = new() { V4 = new(1, 1, 1, 1) };
        
        [UserSetting("Floor Fire Danger Guidance Color")]
        public ScriptColor BladeDangerColor { get; set; } = new() { V4 = new(1, 0, 0, 1) };
        
        private string Phase = "";
        private Vector2? Point1 = new Vector2(0f, 0f);
        private Vector2? Point2 = new Vector2(0f, 0f);
        private Vector2? Point3 = new Vector2(0f, 0f);
        private Vector2? MiddlePoint = new Vector2(0f, 0f);
        private onPoint? OnPoint = null;
        private int bladeCount = 0;
        
        public class Blade
        {
            public UInt32 Id { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Rotation { get; set; }
            public Blade(UInt32 id, double x, double y, double rotation)
            {
                Id = id;
                X = x;
                Y = y;
                Rotation = rotation;
            }
        }
        
        public class onPoint
        {
            public string Name { get; set; }
            public Vector2 OnCoord { get; set; }
            public Vector2 Coord1 { get; set; }
            public Vector2 Coord2 { get; set; }
            public Vector2 Coord3 { get; set; }
            public Vector2 Coord4 { get; set; }
            
            public onPoint(string name, Vector2 onCoord, Vector2 coord1, Vector2 coord2, Vector2 coord3, Vector2 coord4)
            {
                Name = name;
                this.OnCoord = onCoord;
                this.Coord1 = coord1;
                this.Coord2 = coord2;
                this.Coord3 = coord3;
                this.Coord4 = coord4;
            }
        }
        
        private ConcurrentBag<Blade> blades = new ConcurrentBag<Blade>();
        private List<Blade> P1P3Blades = new List<Blade>();
        private List<onPoint> onPoints = new List<onPoint>();
        private List<Vector2?> BladeRoutes;
        
        private void resetPoints()
        {
            onPoints.Clear();
            onPoints.Add(new onPoint("A", new Vector2(100, 93), new Vector2(100, 91.5f), new Vector2(101.4f, 92.9f), new Vector2(100, 94.3f), new Vector2(98.6f, 92.9f)));
            onPoints.Add(new onPoint("B", new Vector2(107, 100), new Vector2(108.5f, 100), new Vector2(107, 101.4f), new Vector2(105.6f, 100), new Vector2(107, 98.6f)));
            onPoints.Add(new onPoint("C", new Vector2(100, 107), new Vector2(100, 108.5f), new Vector2(98.6f, 107), new Vector2(100, 105.6f), new Vector2(101.4f, 107.1f)));
            onPoints.Add(new onPoint("D", new Vector2(93, 100), new Vector2(91.5f, 100), new Vector2(93, 98.6f), new Vector2(94.4f, 100), new Vector2(93, 101.4f)));
        }
        
        public void Init(ScriptAccessory accessory)
        {
            blades.Clear();
            P1P3Blades.Clear();
            BladeRoutes = Enumerable.Repeat<Vector2?>(null, 7).ToList();
            resetPoints();
        }

        public static Vector2? mathPoint(Blade b1, Blade b2)
        {
            float s1 = (float)Math.Sin(b1.Rotation);
            float c1 = (float)Math.Cos(b1.Rotation);
            float s2 = (float)Math.Sin(b2.Rotation);
            float c2 = (float)Math.Cos(b2.Rotation);
    
            float x1 = (float)b1.X;
            float y1 = (float)b1.Y;
            float x2 = (float)b2.X;
            float y2 = (float)b2.Y;

            float d = s1 * c2 - s2 * c1;

            if (Math.Abs(d) < 1e-10) return null;

            float X = (x1 * s1 * c2 - x2 * s2 * c1 - (y2 - y1) * c1 * c2) / d;
            float Y = (y2 * c2 * s1 - y1 * c1 * s2 + (x2 - x1) * s1 * s2) / d;

            return new Vector2(X, Y);
        }

        public static Vector2? middlePoint(Vector2? P1, Vector2? P2)
        {
            if (P1.HasValue && P2.HasValue)
            {
                float midX = (P1.Value.X + P2.Value.X) / 2;
                float midY = (P1.Value.Y + P2.Value.Y) / 2;
                return new Vector2(midX, midY);
            }
            return null;
        }
        
        public static onPoint FindClosestOnPoint(List<onPoint> points, Vector2? target)
        {
            onPoint closestPoint = null;
            float closestDistance = float.MaxValue;
            foreach (var point in points)
            {
                float distance = Vector2.Distance(point.OnCoord, target.Value);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = point;
                }
            }
            return closestPoint;
        }
        
        public static Vector2 FindFarthestPoint(onPoint point, Vector2? referencePoint)
        {
            Vector2[] coords = { point.Coord1, point.Coord2, point.Coord3, point.Coord4 };
            float maxDistance = float.MinValue;
            Vector2 farthestCoord = Vector2.Zero;
            foreach (var coord in coords)
            {
                float distance = Vector2.Distance(coord, referencePoint.Value);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    farthestCoord = coord;
                }
            }
            return farthestCoord;
        }
        
        public static Vector2 FindClosestPoint(onPoint point, Vector2? referencePoint)
        {
            Vector2[] coords = { point.Coord1, point.Coord2, point.Coord3, point.Coord4 };
            float minDistance = float.MaxValue;
            Vector2 closestCoord = Vector2.Zero;
            foreach (var coord in coords)
            {
                float distance = Vector2.Distance(coord, referencePoint.Value);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCoord = coord;
                }
            }
            return closestCoord;
        }

        public static Vector3 Vector3Fucker(Vector2? V)
        {
            Vector3 result = new Vector3();
            if (V.HasValue)
            {
                result.X = V.Value.X;
                result.Y = 0;
                result.Z = V.Value.Y;
            }
            return result;
        }
        
        [ScriptMethod(name: "P5 Floor Fire", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40306"], userControl: false)]
        public void PhaseRecord_P5FloorFire(Event @event, ScriptAccessory accessory)
        {
            Phase = "P5FloorFire";
            blades.Clear();
            P1P3Blades.Clear();
            BladeRoutes.Clear();
            bladeCount = 0;
            BladeRoutes = Enumerable.Repeat<Vector2?>(null, 7).ToList();
            resetPoints();
        }
        
        [ScriptMethod(name: "Debug Switch", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40306"], userControl: true)]
        public void PhaseRecord_P5Debug(Event @event, ScriptAccessory accessory)
        {
            accessory.Method.SendChat($"/e KnightRider wishes you a smooth floor fire!");
        }
        
        private readonly object bladeLock = new object();
        
        [ScriptMethod(name: "Floor Fire Data Capture", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:1"], userControl: false)]
        public void FloorFireDataCapture(Event @event, ScriptAccessory accessory)
        {
            if (Phase == "P5FloorFire")
            {
                lock (bladeLock)
                {
                    if (bladeCount < 7)
                    {
                        var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                        blades.Add(new Blade(
                            id: Convert.ToUInt32(@event["SourceId"], 16),
                            x: Convert.ToDouble(pos.X),
                            y: Convert.ToDouble(pos.Z),
                            rotation: Convert.ToDouble(@event["SourceRotation"])
                        ));
                        bladeCount++;
                    }
                    if (blades.Count == 6)
                    {
                        ProcessBlades();
                    }
                }
            }
        }

        private void ProcessBlades()
        {
            var sortedBlades = blades.OrderBy(b => b.Id).ToList();
            if (sortedBlades != null)
            {
                P1P3Blades.Add(sortedBlades[0]);
                P1P3Blades.Add(sortedBlades[1]);
                P1P3Blades.Add(sortedBlades[4]);
                P1P3Blades.Add(sortedBlades[5]);
                
                Point1 = mathPoint(sortedBlades[0], sortedBlades[1]);
                Point2 = mathPoint(sortedBlades[2], sortedBlades[3]);
                Point3 = mathPoint(sortedBlades[4], sortedBlades[5]);
                MiddlePoint = middlePoint(Point1, Point3);
                OnPoint = FindClosestOnPoint(onPoints, MiddlePoint);
                
                Phase = "P5FloorFireComplete";
            }
        }
        
        private readonly object drawLock = new object();
        
        [ScriptMethod(name: "Floor Fire Data Capture 2", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id2:16"], userControl: false)]
        public void P5_FloorFire2(Event @event, ScriptAccessory accessory)
        {
            if (Phase == "P5FloorFireComplete")
            {
                lock (drawLock)
                {
                    Phase = "P5CalculationComplete";
                    
                    var id = Convert.ToUInt32(@event["SourceId"], 16);
                    Vector2 FarthestPoint = new Vector2();
                    Vector2 ClosestPoint = new Vector2();
                    
                    if (id == P1P3Blades[0].Id || id == P1P3Blades[1].Id)
                    {
                        FarthestPoint = FindFarthestPoint(OnPoint, Point1);
                        ClosestPoint = FindClosestPoint(OnPoint, Point1);
                    }
                    else if (id == P1P3Blades[2].Id || id == P1P3Blades[3].Id)
                    {
                        FarthestPoint = FindFarthestPoint(OnPoint, Point3);
                        ClosestPoint = FindClosestPoint(OnPoint, Point3);
                    }

                    BladeRoutes.Insert(0, FarthestPoint);
                    BladeRoutes.Insert(1, ClosestPoint);
                    BladeRoutes.Insert(2, FindFarthestPoint(OnPoint, Point2));
                    BladeRoutes.Insert(3, FindClosestPoint(OnPoint, Point2));
                    BladeRoutes.Insert(4, ClosestPoint);
                    BladeRoutes.Insert(5, FarthestPoint);

                    int BladeTimes = 2000;
                    
                    // Path 0 -> 1
                    var Goline0 = accessory.Data.GetDefaultDrawProperties();
                    Goline0.Owner = accessory.Data.Me;
                    Goline0.DestoryAt = 9000;
                    Goline0.Color = BladeSafeColor.V4;
                    Goline0.Scale = new(1);
                    Goline0.ScaleMode |= ScaleMode.YByDistance;
                    Goline0.TargetPosition = Vector3Fucker(BladeRoutes[0]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, Goline0);

                    var line1 = accessory.Data.GetDefaultDrawProperties();
                    line1.Position = Vector3Fucker(BladeRoutes[0]);
                    line1.DestoryAt = 9000;
                    line1.Color = BladeDangerColor.V4;
                    line1.Scale = new(1);
                    line1.ScaleMode |= ScaleMode.YByDistance;
                    line1.TargetPosition = Vector3Fucker(BladeRoutes[1]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, line1);
                    
                    // Path 1 -> 2
                    var Goline1 = accessory.Data.GetDefaultDrawProperties();
                    Goline1.Owner = accessory.Data.Me;
                    Goline1.Delay = 9000;
                    Goline1.DestoryAt = BladeTimes;
                    Goline1.Color = BladeSafeColor.V4;
                    Goline1.Scale = new(1);
                    Goline1.ScaleMode |= ScaleMode.YByDistance;
                    Goline1.TargetPosition = Vector3Fucker(BladeRoutes[1]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, Goline1);

                    var line2 = accessory.Data.GetDefaultDrawProperties();
                    line2.Position = Vector3Fucker(BladeRoutes[1]);
                    line2.Delay = 9000;
                    line2.DestoryAt = BladeTimes;
                    line2.Color = BladeDangerColor.V4;
                    line2.Scale = new(1);
                    line2.ScaleMode |= ScaleMode.YByDistance;
                    line2.TargetPosition = Vector3Fucker(BladeRoutes[2]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, line2);

                    // Path 2 -> 3
                    var Goline2 = accessory.Data.GetDefaultDrawProperties();
                    Goline2.Owner = accessory.Data.Me;
                    Goline2.Delay = 9000 + BladeTimes;
                    Goline2.DestoryAt = BladeTimes;
                    Goline2.Color = BladeSafeColor.V4;
                    Goline2.Scale = new(1);
                    Goline2.ScaleMode |= ScaleMode.YByDistance;
                    Goline2.TargetPosition = Vector3Fucker(BladeRoutes[2]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, Goline2);

                    var line3 = accessory.Data.GetDefaultDrawProperties();
                    line3.Position = Vector3Fucker(BladeRoutes[2]);
                    line3.Delay = 9000 + BladeTimes;
                    line3.DestoryAt = BladeTimes;
                    line3.Color = BladeDangerColor.V4;
                    line3.Scale = new(1);
                    line3.ScaleMode |= ScaleMode.YByDistance;
                    line3.TargetPosition = Vector3Fucker(BladeRoutes[3]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, line3);

                    // Path 3 -> 4
                    var Goline3 = accessory.Data.GetDefaultDrawProperties();
                    Goline3.Owner = accessory.Data.Me;
                    Goline3.Delay = 9000 + BladeTimes * 2;
                    Goline3.DestoryAt = BladeTimes;
                    Goline3.Color = BladeSafeColor.V4;
                    Goline3.Scale = new(1);
                    Goline3.ScaleMode |= ScaleMode.YByDistance;
                    Goline3.TargetPosition = Vector3Fucker(BladeRoutes[3]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, Goline3);

                    var line4 = accessory.Data.GetDefaultDrawProperties();
                    line4.Position = Vector3Fucker(BladeRoutes[3]);
                    line4.Delay = 9000 + BladeTimes * 2;
                    line4.DestoryAt = BladeTimes;
                    line4.Color = BladeDangerColor.V4;
                    line4.Scale = new(1);
                    line4.ScaleMode |= ScaleMode.YByDistance;
                    line4.TargetPosition = Vector3Fucker(BladeRoutes[4]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, line4);

                    // Path 4 -> 5
                    var Goline4 = accessory.Data.GetDefaultDrawProperties();
                    Goline4.Owner = accessory.Data.Me;
                    Goline4.Delay = 9000 + BladeTimes * 3;
                    Goline4.DestoryAt = BladeTimes;
                    Goline4.Color = BladeSafeColor.V4;
                    Goline4.Scale = new(1);
                    Goline4.ScaleMode |= ScaleMode.YByDistance;
                    Goline4.TargetPosition = Vector3Fucker(BladeRoutes[4]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, Goline4);

                    var line5 = accessory.Data.GetDefaultDrawProperties();
                    line5.Position = Vector3Fucker(BladeRoutes[4]);
                    line5.Delay = 9000 + BladeTimes * 3;
                    line5.DestoryAt = BladeTimes;
                    line5.Color = BladeDangerColor.V4;
                    line5.Scale = new(1);
                    line5.ScaleMode |= ScaleMode.YByDistance;
                    line5.TargetPosition = Vector3Fucker(BladeRoutes[5]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, line5);

                    // Path 5 -> end
                    var Goline5 = accessory.Data.GetDefaultDrawProperties();
                    Goline5.Owner = accessory.Data.Me;
                    Goline5.Delay = 9000 + BladeTimes * 4;
                    Goline5.DestoryAt = BladeTimes;
                    Goline5.Color = BladeSafeColor.V4;
                    Goline5.Scale = new(1);
                    Goline5.ScaleMode |= ScaleMode.YByDistance;
                    Goline5.TargetPosition = Vector3Fucker(BladeRoutes[5]);
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, Goline5);
                }
            }
        }
    }
}