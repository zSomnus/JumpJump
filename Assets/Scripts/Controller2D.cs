using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityOverride
{
    public string Source { get; set; }
    public Vector3 Velocity { get; set; }
    public float Drag { get; set; }
    public bool IsAllowingGravity { get; }
    public bool IsClearingForce { get; }
    public VelocityOverride(string source, Vector3 velocity, float drag = 50.0f, bool isAllowingGravity = false, bool isClearingTempForce = false)
    {
        Source = source;
        Velocity = velocity;
        Drag = drag;
        IsAllowingGravity = isAllowingGravity;
        IsClearingForce = isClearingTempForce;
    }
}
public class Controller2D
{
    float maxJumpVelocity;
    float minJumpVelocity;
    float gravity;
    LayerMask obstacleMask;
    LayerMask actorMask;
    const float ColliderPadding = 0.015f;
    const float TempForceDecay = 20.0f;
    public int horizontalRayCount = 1;
    public int verticalRayCount = 2;
    float maxClimbAngle = 80;
    float maxDescendAngle = 80;
    float horizontalRaySpacing;
    float verticalRaySpacing;
    BoxCollider2D boxCollider2D;
    RaycastOrigins raycastOrigins;
    Vector3 localColliderOffset;
    Bounds cachedBounds;

    public CollisionInfo collisions;
    protected Vector3 velocity;
    Vector3 TotalTempForce => volatileTempForce + permanentTempForce;

    Vector3 volatileTempForce;
    Vector3 permanentTempForce;

    Actor owner;
    public Actor Owner { get => owner; set => owner = value; }
    public GameObject OwnerObject { get; set; }
    float DropTimeCap = 0.15f;
    float dropTime;
    bool IsDroppingThrough { get; set; }
    public bool IsDroppable { get; private set; }
    public bool IsGravityOn { get; set; } = true;

    public bool IsFlying
    {
        get => _isFlying;
        set
        {
            _isFlying = value;
            IsGravityOn = !value;
        }
    }

    bool _isNoClip;
    public bool IsNoClip
    {
        get => _isNoClip;
        set
        {
            _isNoClip = value;

            if (value)
            {
                velocity = Vector3.zero;
            }
        }
    }

    public float Gravity
    {
        get => gravity * (IsJuggled ? JuggleGravityFactor : 1);
        set => gravity = value;
    }

    const float JuggleGravityFactor = 0.5f;
    public bool IsJuggled { get; private set; }
    public GameObject OwnerObjet { get; private set; }

    const float JuggleMinDuration = 0.5f;
    float juggleMinTimer;

    PlatformController movingPlatform;
    VelocityOverride velocityOverride = null;
    bool isFalling;
    bool _isFlying;
    const float VelocityOverrideEndThreshold = 10f;
    const float FrontObstacleRayDistance = 0.5f;
    const int FlyingIdleDrag = 100;
    const float DefaultMoveSpeed = 10;

    RaycastHit2D[] rayCache = new RaycastHit2D[8];
    RaycastHit2D hit;
    bool canMoveForwardGroundedCacheSet;
    bool canMoveForwardGroundedCacheResult;

    public virtual void Init(BoxCollider2D boxCollider2D, Actor owner, bool isFlying = false, int? horizontalRayCount = null, int? verticalRayCount = null)
    {
        if (horizontalRayCount.HasValue)
        {
            this.horizontalRayCount = horizontalRayCount.Value;
        }

        if (verticalRayCount.HasValue)
        {
            this.verticalRayCount = verticalRayCount.Value;
        }
        IsFlying = isFlying;
        this.owner = owner;
        this.boxCollider2D = boxCollider2D;
        localColliderOffset = boxCollider2D.bounds.center - (owner?.transform?.position ?? OwnerObject.transform.position);
        cachedBounds = boxCollider2D.bounds;

        string ownerObstacleMask = "";

        if (owner != null && owner.CompareTag("Player"))
        {
            ownerObstacleMask = "BlockPlayer";
        }
        else
        {
            ownerObstacleMask = IsFlying ? "BlockEnemyFlyer" : "BlockEnemy";
        }

        actorMask = MaskHelper.GetLayerMask(ownerObstacleMask);
        obstacleMask = MaskHelper.GetLayerMask("Obstacle");
        CalculateSpacing();
        SetJumpValues();
    }

    public void UpdateGravity()
    {
        if (!IsGravityOn)
        {
            return;
        }

        if (velocityOverride != null)// && !velocityOverride.IsAllowingGravity)
        {
            return;
        }

        if (TotalTempForce.magnitude > 0.01f)
        {
            return;
        }

        velocity.y += Gravity * GameTime.deltaTime;
    }

    private void SetJumpValues(float maxJumpHeight = 4, float minJumpHeight = 1, float timeToJumpApex = 0.4f)
    {
        Gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(Gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(Gravity) * minJumpHeight);
    }

    private void CalculateSpacing()
    {
        Bounds bounds = boxCollider2D.bounds;
        bounds.Expand(ColliderPadding * -2);

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 1, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 1, int.MaxValue);

        float horizontalSpacingCount = Mathf.Max(1, horizontalRayCount - 1);
        float verticalSpacingCount = Math.Max(1, verticalRayCount - 1);
        horizontalRaySpacing = bounds.size.y / horizontalSpacingCount;
        verticalRaySpacing = bounds.size.x / verticalSpacingCount;
    }

    public virtual void Update()
    {
        canMoveForwardGroundedCacheSet = false;
        UpdateJuggle();
        UpdateVelocityOverride();

        if (dropTime > 0)
        {
            dropTime -= Time.deltaTime;
            if (dropTime <= 0)
            {
                IsDroppingThrough = false;
            }
        }
    }

    public bool IsGrounded()
    {
        return collisions.below;
    }

    internal bool IsFalling()
    {
        return isFalling;
    }

    private void UpdateVelocityOverride()
    {
        if (velocityOverride != null)
        {
            float velocityMag = velocity.magnitude;
            velocityMag = Mathf.Max(velocityMag - velocityOverride.Drag * Time.deltaTime, 0);
            velocity = velocity.normalized * velocityMag;

            if (velocityMag < VelocityOverrideEndThreshold)
            {
                OnVelocityOverrideFinish(velocityOverride);
            }
        }
    }

    protected virtual void OnVelocityOverrideFinish(VelocityOverride velocityOverride)
    {
        this.velocityOverride = null;
    }

    public void StartJump(float ratio = 1)
    {
        if (collisions.below)
        {
            ForceJump(ratio);
        }
    }

    public void ForceJump(float ratio = 1)
    {
        if (IsNoClip)
        {
            return;
        }

        if (owner != null && owner.TimeScalePercent > 0)
        {
            velocity.y = maxJumpVelocity * ratio;
        }
    }

    public void EndJump()
    {
        if (velocityOverride == null && velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }

    internal void AddTemporaryForce(Vector2 force, bool isVolatile = true)
    {
        if (isVolatile)
        {
            volatileTempForce += new Vector3(force.x, force.y, 0);
        }
        else
        {
            permanentTempForce += new Vector3(force.x, force.y, 0);
        }
    }

    public void ResetVelocity()
    {
        velocity = Vector3.zero;
        velocityOverride = null;
        permanentTempForce = Vector3.zero;
        volatileTempForce = Vector3.zero;
    }

    public virtual void Reset()
    {

    }

    public void Move(Vector2 input)
    {
        if (!IsNoClip)
        {
            if (velocity.y > 0)
            {
                if (collisions.above)
                {
                    velocity.y = 0;
                }
            }
            else
            {
                if (collisions.below)
                {
                    velocity.y = 0;
                }
            }

            UpdateGravity();

            if (IsFlying)
            {
                float velocityMag = velocity.magnitude;
                velocityMag = Mathf.Max(velocityMag - FlyingIdleDrag * GameTime.deltaTime, 0);
                velocity = velocity.normalized * velocityMag;
            }
        }

        CheckVelocityCollision();

        float targetVelocityX = input.x * GetMoveSpeed();
        Vector3 inputDelta = new Vector3(targetVelocityX, 0);

        if (IsNoClip || IsFlying)
        {
            inputDelta.y += input.y * GetMoveSpeed();
        }

        Vector3 moveDelta = velocity * GameTime.deltaTime + TotalTempForce * GameTime.deltaTime;
        volatileTempForce = volatileTempForce.normalized * Mathf.Max(volatileTempForce.magnitude - GameTime.deltaTime * TempForceDecay, 0);
        permanentTempForce = permanentTempForce.normalized * Mathf.Max(permanentTempForce.magnitude - GameTime.deltaTime * TempForceDecay, 0);

        //CheckVelocityCollision();

        if (IsNoClip)
        {
            volatileTempForce = Vector3.zero;
            permanentTempForce = Vector3.zero;
        }

        moveDelta = ApplyInput(moveDelta, inputDelta);

        if (!IsNoClip)
        {
            UpdateRaycastOrigins();
            collisions.Reset();
            collisions.moveDeltaOld = moveDelta;

            if (moveDelta.y <= 0 && !IsDroppingThrough)
            {
                DescendSlope(ref moveDelta);
            }

            isFalling = moveDelta.y <= 0;

            if (moveDelta.x != 0)
            {
                HorizontalCollisions(ref moveDelta);
            }
            if (moveDelta.y != 0)
            {
                VerticalCollisions(ref moveDelta);
            }
        }

        if (owner != null)
        {
            owner.transform.Translate(moveDelta);
        }
        else if (OwnerObjet != null)
        {
            OwnerObjet.transform.Translate(moveDelta);
        }
    }

    private void VerticalCollisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + ColliderPadding;
        bool anyMovingPlatformHit = false;
        IsDroppable = true;
        bool isHitAnything = false;
        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            if (verticalRayCount == 1)
            {
                rayOrigin.x += verticalRaySpacing / 2f;
            }
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, actorMask);
            //Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit)
            {
                if (!isHitAnything)
                {
                    UpdateGround(hit.collider);
                    isHitAnything = true;
                }
                else
                {
                    UpdateSecondGround(hit.collider);
                }

                bool isPlatform = hit.collider.CompareTag("Platform");
                IsDroppable &= isPlatform;
                if (isPlatform)
                {
                    if (hit.distance <= 0 || directionY > 0 || IsDroppingThrough)
                        continue;
                }
                velocity.y = (hit.distance - ColliderPadding) * directionY;
                rayLength = hit.distance;

                if (collisions.climbingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }
                collisions.below = directionY == -1;
                collisions.above = directionY == 1;

                if (directionY < 0 && hit.collider.CompareTag("MovingObstacle"))
                {
                    anyMovingPlatformHit = true;
                    if (movingPlatform != null && movingPlatform.gameObject != hit.collider.gameObject)
                    {
                        movingPlatform.RemovePassenger(owner);
                    }
                    if (movingPlatform == null || movingPlatform.gameObject != hit.collider.gameObject)
                    {
                        movingPlatform = hit.collider.GetComponent<PlatformController>();
                        movingPlatform.AddPassenger(owner);
                    }
                }
            }
        }

        IsDroppable &= isHitAnything;

        if (!anyMovingPlatformHit && movingPlatform != null)
        {
            movingPlatform.RemovePassenger(owner);
            movingPlatform = null;
        }

        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + ColliderPadding;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
            hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, GetActiveLayer());

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle)
                {
                    velocity.x = (hit.distance - ColliderPadding) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }

        if (IsDroppingThrough)
        {
            if (!isHitAnything)
            {
                dropTime = 0;
                IsDroppingThrough = false;
            }
        }
    }

    LayerMask GetActiveLayer()
    {
        return IsDroppingThrough ? obstacleMask : actorMask;
    }

    protected virtual void UpdateSecondGround(Collider2D collider)
    {
    }

    protected virtual void UpdateGround(Collider2D collider)
    {
    }

    private void HorizontalCollisions(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + ColliderPadding;

        bool isFirstHit = true;

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            int rayCount = Physics2D.RaycastNonAlloc(rayOrigin, Vector2.right * directionX, rayCache, rayLength, GetActiveLayer());
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (rayCount > 0)
            {
                for (int r = 0; r < rayCount; r++)
                {
                    hit = rayCache[r];
                    bool isPlatform = hit.collider.CompareTag("Platform");

                    if (isPlatform)
                    {
                        if (IsDroppingThrough || hit.distance <= 0)
                        {
                            continue;
                        }
                    }

                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    if (i == 0 && slopeAngle <= maxClimbAngle)
                    {
                        if (collisions.descendingSlope)
                        {
                            collisions.descendingSlope = false;
                            velocity = collisions.moveDeltaOld;
                        }
                        float distanceToSlopeStart = 0;
                        if (slopeAngle != collisions.slopeAngleOld)
                        {
                            distanceToSlopeStart = hit.distance - ColliderPadding;
                            velocity.x -= distanceToSlopeStart * directionX;
                        }

                        if (isFirstHit)
                        {
                            isFirstHit = false;
                            UpdateGround(hit.collider);
                        }

                        ClimbSlope(ref velocity, slopeAngle);
                        velocity.x += distanceToSlopeStart * directionX;
                    }

                    if (isPlatform)
                    {
                        continue;
                    }

                    if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
                    {
                        velocity.x = (hit.distance - ColliderPadding) * directionX;
                        rayLength = hit.distance;

                        if (collisions.climbingSlope)
                        {
                            velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                        }

                        collisions.left = directionX == -1;
                        collisions.right = directionX == 1;
                    }
                }
            }
        }
    }

    private void ClimbSlope(ref Vector3 velocity, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
    }

    private void DescendSlope(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        hit = Physics2D.Raycast(rayOrigin, -Vector2.up, 1, GetActiveLayer());

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
            {
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    if (hit.distance - ColliderPadding <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;
                    }
                }
            }
        }
    }

    protected void UpdateRaycastOrigins()
    {
        Bounds bounds = cachedBounds;
        bounds.center = (owner != null ? owner.transform.position : OwnerObjet.transform.position) + localColliderOffset;

        bounds.Expand(ColliderPadding * -2);

        float minX = bounds.min.x;
        float minY = bounds.min.y;
        float maxX = bounds.max.x;
        float maxY = bounds.max.y;

        raycastOrigins.bottomLeft = new Vector2(minX, minY);
        raycastOrigins.bottomRight = new Vector2(maxX, minY);
        raycastOrigins.topLeft = new Vector2(minX, maxY);
        raycastOrigins.topRight = new Vector2(maxX, maxY);
    }

    public Vector3 ApplyInput(Vector3 moveDelta, Vector3 inputDelta)
    {
        float ownerTime = owner != null ? owner.MyDeltaTime : Time.deltaTime;
        return velocityOverride == null ? moveDelta + inputDelta * ownerTime : moveDelta;
    }

    private float GetMoveSpeed()
    {
        return owner != null ? owner.GetMoveSpeed() : DefaultMoveSpeed;
    }

    private void CheckVelocityCollision()
    {
        if (IsColliding())
        {
            velocity.x = 0;

            if (IsFlying)
            {
                velocity.y = 0;
            }
        }
    }

    private bool IsColliding()
    {
        if (IsNoClip)
        {
            return false;
        }

        return (velocity.y > 0 && collisions.above) ||
            (velocity.y < 0 && collisions.below) ||
            (velocity.x > 0 && collisions.right) ||
            (velocity.x < 0 && collisions.left);
    }

    private void UpdateJuggle()
    {
        if (juggleMinTimer > 0)
        {
            juggleMinTimer -= Time.deltaTime;
        }
        else if (IsGrounded())
        {
            EndJuggle();
        }
    }

    public void EndJuggle()
    {
        if (owner != null)
        {
            owner.EndJuggle();
        }
        IsJuggled = false;
    }

    public bool IsInsideWalls()
    {
        var pos = Owner != null ? Owner.transform.position : OwnerObject.transform.position;
        hit = Physics2D.Raycast(pos, Vector2.right, 1, obstacleMask);

        if (hit.collider != null)
        {
            hit = Physics2D.Raycast(pos, Vector2.left, 1, obstacleMask);
            return hit.collider != null;
        }

        return false;
    }

    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    public struct CollisionInfo
    {

        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public float slopeAngle, slopeAngleOld;
        public Vector3 moveDeltaOld;

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }

    internal void Clear()
    {
        velocity = Vector3.zero;
        volatileTempForce = Vector3.zero;
        permanentTempForce = Vector3.zero;
        velocityOverride = null;
    }
}
