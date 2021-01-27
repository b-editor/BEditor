﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using BEditor.Core.Command;
using BEditor.Core.Data.Property;
using BEditor.Core.Extensions;
using BEditor.Core.Graphics;
using BEditor.Core.Properties;

namespace BEditor.Core.Data.Primitive.Objects
{
    [DataContract]
    public class CameraObject : ObjectElement
    {
        public static readonly EasePropertyMetadata XMetadata = new(Resources.X, 0, UseOptional: true);
        public static readonly EasePropertyMetadata YMetadata = new(Resources.Y, 0, UseOptional: true);
        public static readonly EasePropertyMetadata ZMetadata = new(Resources.Z, 1024, UseOptional: true);
        public static readonly EasePropertyMetadata TargetXMetadata = new(Resources.TargetX, 0, UseOptional: true);
        public static readonly EasePropertyMetadata TargetYMetadata = new(Resources.TargetY, 0, UseOptional: true);
        public static readonly EasePropertyMetadata TargetZMetadata = new(Resources.TargetZ, 0, UseOptional: true);
        public static readonly EasePropertyMetadata ZNearMetadata = new(Resources.ZNear, 0.1F, Min: 0.1F, UseOptional: true);
        public static readonly EasePropertyMetadata ZFarMetadata = new(Resources.ZFar, 20000, UseOptional: true);
        public static readonly EasePropertyMetadata AngleMetadata = new(Resources.Angle, 0, UseOptional: true);
        public static readonly EasePropertyMetadata FovMetadata = new(Resources.Fov, 45, 45, 1, UseOptional: true);
        public static readonly CheckPropertyMetadata ModeMetadata = new(Resources.Perspective, true);

        public CameraObject()
        {
            X = new(XMetadata);
            Y = new(YMetadata);
            Z = new(ZMetadata);
            TargetX = new(TargetXMetadata);
            TargetY = new(TargetYMetadata);
            TargetZ = new(TargetZMetadata);
            ZNear = new(ZNearMetadata);
            ZFar = new(ZFarMetadata);
            Angle = new(AngleMetadata);
            Fov = new(FovMetadata);
            Mode = new(ModeMetadata);
        }

        public override string Name => Resources.Camera;
        public override IEnumerable<PropertyElement> Properties => new PropertyElement[]
        {
            X, Y, Z,
            TargetX, TargetY, TargetZ,
            ZNear, ZFar,
            Angle,
            Fov,
            Mode
        };
        [DataMember(Order = 0)]
        public EaseProperty X { get; private set; }
        [DataMember(Order = 1)]
        public EaseProperty Y { get; private set; }
        [DataMember(Order = 2)]
        public EaseProperty Z { get; private set; }
        [DataMember(Order = 3)]
        public EaseProperty TargetX { get; private set; }
        [DataMember(Order = 4)]
        public EaseProperty TargetY { get; private set; }
        [DataMember(Order = 5)]
        public EaseProperty TargetZ { get; private set; }
        [DataMember(Order = 6)]
        public EaseProperty ZNear { get; private set; }
        [DataMember(Order = 7)]
        public EaseProperty ZFar { get; private set; }
        [DataMember(Order = 8)]
        public EaseProperty Angle { get; private set; }
        [DataMember(Order = 9)]
        public EaseProperty Fov { get; private set; }
        [DataMember(Order = 10)]
        public CheckProperty Mode { get; private set; }

        public override void Render(EffectRenderArgs args)
        {
            int frame = args.Frame;
            var scene = Parent!.Parent!;
            scene.GraphicsContext!.MakeCurrent();

            if (Mode.IsChecked)
            {
                scene.GraphicsContext.Camera =
                    new PerspectiveCamera(new(X[frame], Y[frame], Z[frame]), scene.Width / (float)scene.Height)
                    {
                        Far = ZFar[frame],
                        Near = ZNear[frame],
                        Fov = Fov[frame],
                        Target = new(TargetX[frame], TargetY[frame], TargetZ[frame])
                    };
            }
            else
            {
                scene.GraphicsContext.Camera =
                    new OrthographicCamera(new(X[frame], Y[frame], Z[frame]), scene.Width, scene.Height)
                    {
                        Far = ZFar[frame],
                        Near = ZNear[frame],
                        Fov = Fov[frame],
                        Target = new(TargetX[frame], TargetY[frame], TargetZ[frame])
                    };
            }

            var list = Parent.Effect.Where(e => e.IsEnabled).ToArray();
            for (int i = 1; i < list.Length; i++)
            {
                var effect = list[i];

                effect.Render(args);
            }
        }
        protected override void OnLoad()
        {
            X.Load(XMetadata);
            Y.Load(YMetadata);
            Z.Load(ZMetadata);
            TargetX.Load(TargetXMetadata);
            TargetY.Load(TargetYMetadata);
            TargetZ.Load(TargetZMetadata);
            ZNear.Load(ZNearMetadata);
            ZFar.Load(ZFarMetadata);
            Angle.Load(AngleMetadata);
            Fov.Load(FovMetadata);
            Mode.Load(ModeMetadata);
        }
        protected override void OnUnload()
        {
            X.Unload();
            Y.Unload();
            Z.Unload();
            TargetX.Unload();
            TargetY.Unload();
            TargetZ.Unload();
            ZNear.Unload();
            ZFar.Unload();
            Angle.Unload();
            Fov.Unload();
            Mode.Unload();
        }
    }
}