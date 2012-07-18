using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TrackModelProcessor;
using System.Collections;

namespace Kreen.MainGame
{
    static class CollisionDetection
    {
        private static Hashtable taggedSegments;
        private static string lastSegment;

        public static void setupTaggedSegments(Track track)
        {
            taggedSegments = new Hashtable();

            foreach (ModelMesh mesh in track.TrackModel.Meshes)
            {
                taggedSegments.Add(mesh.Name, false);
            }
        }

        public static float? CheckWheelTrackCollision(Car.Wheel wheel, Track track, Vector3 movementVector)
        {

            float? minDistance = float.MaxValue;

            // Iterate over meshes
            foreach (ModelMesh mesh in track.TrackModel.Meshes)
            {
                // Ignore all track meshes not starting with c_
               /* if (!mesh.Name.StartsWith("c_"))
                    continue; */

                BoundingBox trackSegmentBB = track.BoundingBoxes[mesh.ParentBone.Index];

                // Check box level collision first for performance considerations
                if (CheckBoxCollision(wheel.Box, trackSegmentBB))
                {
                    taggedSegments.Remove(mesh.Name);
                    taggedSegments.Add(mesh.Name, true);
                    lastSegment = mesh.Name;

                    // Do in-depth ray-triangle check
                    float? distance = InDepthTrackCollisionDetection(mesh, wheel.Position, movementVector, track);

                    if (distance.HasValue)
                    {
                        if (distance < minDistance)
                            minDistance = distance;
                    }
                }
                       
            }

            if (minDistance == float.MaxValue)
            {
                return null;
            }

            return minDistance;
        }

        private static bool CheckBoxCollision(BoundingBox wheelBox, BoundingBox trackBox)
        {
            Vector3 min = new Vector3(Math.Min(trackBox.Min.X, trackBox.Max.X), Math.Min(trackBox.Min.Y, trackBox.Max.Y), Math.Min(trackBox.Min.Z, trackBox.Max.Z));
            Vector3 max = new Vector3(Math.Max(trackBox.Min.X, trackBox.Max.X), Math.Max(trackBox.Min.Y, trackBox.Max.Y), Math.Max(trackBox.Min.Z, trackBox.Max.Z));

            // 2D plane only
            foreach (Vector3 corner in wheelBox.GetCorners())
            {
                if ((corner.X > min.X && corner.X < max.X) &&
                    (corner.Z > min.Z && corner.Z < max.Z) &&
                    (corner.Y > min.Y - 10 && corner.Y < max.Y + 10))
                    return true;
            }

            return false;
        }

        private static float? InDepthTrackCollisionDetection(ModelMesh mesh, Vector3 boundingPoint, Vector3 movementVector, Track track)
        {
            Matrix absoluteTransform = track.Transforms[mesh.ParentBone.Index];
            TrackModelProcessor.MeshInfo.Triangle[] triangles = ((MeshInfo) mesh.Tag).Triangles.ToArray();

            // Create ray
            Ray ray = new Ray(boundingPoint, movementVector);

            float? minDistance = float.MaxValue;

            // Iterate over triangles and find minimum distance
            foreach (TrackModelProcessor.MeshInfo.Triangle triangle in triangles)
            {
                Vector3 p0 = triangle.P0;
                Vector3 p1 = triangle.P1;
                Vector3 p2 = triangle.P2;

                Plane trianglePlane = new Plane(p0, p1, p2);

                // Calculate distance on ray
                float distance = RayPlaneDistance(ray, trianglePlane);

                Vector3 intersectionPoint = ray.Position + ray.Direction * distance;

                // Ignore if the ray-plane intersection point isn't inside the triangle
                if (!PointInsideTriangle(p0, p1, p2, intersectionPoint))
                    continue;


                distance = distance * movementVector.Length();

                if (minDistance > distance)
                    minDistance = distance;
            }

            if (minDistance == float.MaxValue)
                return null;
            else
                return minDistance;
        }

        /// <summary>
        /// Calculates distance between ray and plane
        /// </summary>
        private static float RayPlaneDistance(Ray ray, Plane plane)
        {
            float rayPointDistance = -plane.DotNormal(ray.Position);
            float rayPointToPlaneDistance = rayPointDistance - plane.D;
            float directionProjectedLength = Vector3.Dot(plane.Normal, ray.Direction);

            float factor = rayPointToPlaneDistance / directionProjectedLength;

            return factor;
        }

        /// <summary>
        /// Checks if the point is inside the triangle
        /// </summary>
        private static bool PointInsideTriangle(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 intersectionPoint)
        {
            Vector3 A0 = intersectionPoint - p0;
            Vector3 B0 = p1 - p0;
            Vector3 cross0 = Vector3.Cross(A0, B0);

            Vector3 A1 = intersectionPoint - p1;
            Vector3 B1 = p2 - p1;
            Vector3 cross1 = Vector3.Cross(A1, B1);

            Vector3 A2 = intersectionPoint - p2;
            Vector3 B2 = p0 - p2;
            Vector3 cross2 = Vector3.Cross(A2, B2);

            if (CompareSigns(cross0, cross1) && CompareSigns(cross0, cross2))
            {
                return true;
            }

            return false;
        }

        private static bool CompareSigns(Vector3 first, Vector3 second)
        {
            if (Vector3.Dot(first, second) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool LapDone()
        {
            Boolean done = true;

            foreach (Boolean value in taggedSegments.Values)
            {
                done = done && value;
            }

            return done && lastSegment == "c_02";
        }
    }
}
