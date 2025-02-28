using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;


public class PlayerBot : Player
{

    [Header("Bot Config")]
    [SerializeField] private float range = 6f;
    [SerializeField] private float detectRange = 6f;
    [SerializeField] private float attackRange = 6f;
    [SerializeField] private float stuckTimeCheck = 2.0f;
    [SerializeField] private float stuckDistance = 1.0f;
    [SerializeField] private float followTargetDistance = 1.0f;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private LayerMask hitMask;


    //list of enemy players that are in range of this bot
    private List<Player> inRange = new List<Player>();

    //reference to the agent component

    //current destination on the navigation mesh
    private Vector3 targetPoint;
    private NavMeshPath path;
    private int _curPathIndex;

    private float _checkStuckTimer;
    private Vector3 _previousPos;
    private bool _isStucked = false;

    private bool _isFollowTarget;
    private bool _isPredictShoot;

    private float _countUpdate;

    public override void Spawned()
    {
        base.Spawned();
        path = new NavMeshPath();

        targetPoint = LevelController.Instance.GetSpawnPosition(NetworkedTeamData.TeamId);
        targetPoint.y = 0;
        _countUpdate = 0;

        _isFollowTarget = true;
    }

    //sets inRange list for player detection
    void DetectPlayers()
    {
        if(!_isFollowTarget)
        {
            inRange.Clear();
        }

        for(int i = inRange.Count - 1; i >= 0; i--)
        {
            if (inRange[i] == null || !inRange[i].IsActivated)
            {
                inRange.RemoveAt(i);
            }
        }


        //casts a sphere to detect other player objects within the sphere radius
        Collider[] cols = Physics.OverlapSphere(transform.position, detectRange, hitMask);
        //loop over players found within bot radius
        Debug.Log("Detect Player: " + cols.Length);
        for (int i = 0; i < cols.Length; i++)
        {
            //get other Player component
            //only add the player to the list if its not in this team
            Player p = cols[i].gameObject.GetComponent<Player>();
            Debug.Log(p.gameObject.name);
            if (NetworkedTeamData.TeamId != p.NetworkedTeamData.TeamId && !inRange.Contains(p) && p.state == PlayerState.Active)
            {
                Debug.Log(p.gameObject.name + "--- Add Range");
                inRange.Add(p);
            }
        }
    }


    //calculate random point for movement on navigation mesh
    private void RandomPoint(Vector3 center, float limitRange, out Vector3 result)
    {
        center.y = 0;
        //clear previous target point
        result = Vector3.zero;
        //try to find a valid point on the navmesh with an upper limit (10 times)
        for (int i = 0; i < 10; i++)
        {
            //find a point in the movement radius
            Vector2 randomCircle = Random.insideUnitCircle;
            Vector3 randomPoint = center + new Vector3(randomCircle.x, 0, randomCircle.y) * limitRange;
            randomPoint.y = 0;
            NavMeshHit hit;
            //if the point found is a valid target point, set it and continue
            if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.AllAreas))
            {
                result = hit.position;
                break;
            }
        }

        //set the target point as the new destination
        NavMesh.CalculatePath(transform.position, result, NavMesh.AllAreas, path);
        _curPathIndex = 0;

        _isStucked = false;
        _checkStuckTimer = 0;
        _previousPos = transform.position;
        //agent.SetDestination(result);
    }



    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        if(state != PlayerState.Active)
        {
            return;
        }

        if(_countUpdate >= 0)
        {
            _countUpdate += Runner.DeltaTime;
            if(_countUpdate >= 1)
            {
                _countUpdate = 0;
                DetectPlayers();
            }
        }

        for (int i = 0; i < path.corners.Length - 1; i++)
            Debug.DrawLine(path.corners[i] + Vector3.up, path.corners[i + 1] + Vector3.up, Color.red);

        Vector2 aimDirection = Vector3.zero;

        if(CheckDistance(_previousPos, transform.position, stuckDistance))
        {
            if(_checkStuckTimer > stuckTimeCheck)
            {
                _isStucked =true;
                _checkStuckTimer = 0;
            } else
            {
                _checkStuckTimer += Time.deltaTime;
            }
        } else
        {
            _isStucked = false;
            _checkStuckTimer = 0;
        }

        Player target = GetPlayerTarget();
        //no enemy players are in range
        if (target == null)
        {
            //if this bot reached the the random point on the navigation mesh,
            //then calculate another random point on the navmesh on continue moving around
            //with no other players in range, the AI wanders from team spawn to team spawn
            if (CheckDistance(transform.position, targetPoint, agent.stoppingDistance * 2) || _isStucked)
            {
                RandomPoint(LevelController.Instance.GetRandomSpawnPoint(), range, out targetPoint);
            }
        }
        else
        {
            //if we reached the targeted point, calculate a new point around the enemy
            //this simulates more fluent "dancing" movement to avoid being shot easily
            if ( _isStucked)
            {
                RandomPoint(target.transform.position, attackRange, out targetPoint);
            }
            else
            {
                if (_isFollowTarget)
                {
                    if(CheckDistance(transform.position, targetPoint, agent.stoppingDistance * 2))
                    {
                        RandomPoint(target.transform.position, attackRange, out targetPoint);
                    }
                }
                else
                {
                    if ((CheckDistance(transform.position, targetPoint, agent.stoppingDistance * 2)))
                    {
                        RandomPoint(target.transform.position, attackRange, out targetPoint);
                    }
                }
            }

            if (!_isFollowTarget)
            {
                Collider[] cols = Physics.OverlapSphere(transform.position, attackRange, hitMask);

                for(int i = 0; i < cols.Length; i++)
                {
                    Player p = cols[i].gameObject.GetComponent<Player>();
                    if (NetworkedTeamData.TeamId != p.NetworkedTeamData.TeamId && !inRange.Contains(p) && p.state == PlayerState.Active)
                    {
                        target = p;
                        break;
                    }
                }

            }

            //raycast to detect visible enemies and shoot at their current position
            if (target != null)
            {

                if (!_isPredictShoot)
                {
                    Vector3 lookPos = target.gameObject.transform.position;
                    Vector3 tempDir = lookPos - transform.position;
                    aimDirection = new Vector2(tempDir.x, tempDir.z);
                }
                else
                {
                    Vector3 lookPos = PredictEnemyPosition(target.transform.position, target.Velocity);
                    Vector3 tempDir = lookPos - transform.position;
                    aimDirection = new Vector2(tempDir.x, tempDir.z);
                }


                weaponMgr.FireWeapon(WeaponManager.WeaponInstallationType.Primary, aimDirection);
                weaponMgr.FireWeapon(WeaponManager.WeaponInstallationType.Secondary, aimDirection);
            }
        }

        Vector2 moveDirection = Vector2.zero;
        if (path != null && _curPathIndex < path.corners.Length)
        {
            Vector3 nextPos = path.corners[_curPathIndex];
            nextPos.y = transform.position.y;
            if (CheckDistance(transform.position, path.corners[_curPathIndex], agent.stoppingDistance * 2))
            {
                _curPathIndex++;
            }

            if (_curPathIndex < path.corners.Length)
            {
                Vector3 temp = nextPos - transform.position;
                moveDirection = new Vector2(temp.x, temp.z);
                Debug.DrawLine(transform.position, path.corners[_curPathIndex], Color.green);
                Debug.DrawLine(transform.position, (path.corners[_curPathIndex] - transform.position).normalized * 2, Color.blue);
            }
        }

        _previousPos = transform.position;
        SetMoveDirections(moveDirection.normalized);
    }

    private Player GetPlayerTarget()
    {
        if (inRange.Count == 0) return null;

        for(int i = inRange.Count - 1; i >= 0; i--)
        {
            if (inRange[i] == null || inRange[i].state != PlayerState.Active)
            {
                inRange.RemoveAt(i);
            }
        }

        if(inRange.Count == 0) return null;

        return inRange[0];
    }

    private Vector3 PredictEnemyPosition(Vector3 enemyPosition, Vector3 enemyVelocity)
    {
        Vector3 shooterPosition = transform.position;
        Vector3 toEnemy = enemyPosition - shooterPosition;
        float distanceToEnemy = toEnemy.magnitude;
        float timeToTarget = distanceToEnemy / weaponMgr.primaryWeaponBulletSpeed;

        Vector3 predictedPosition = enemyPosition + enemyVelocity * timeToTarget;
        return predictedPosition;
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetPoint, 1);

        if (path != null && _curPathIndex < path.corners.Length)
        {
            if (CheckDistance(transform.position, path.corners[_curPathIndex], agent.stoppingDistance * 2))
            {
                _curPathIndex++;
            }

            if (_curPathIndex < path.corners.Length)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(path.corners[_curPathIndex], 1);
            }
        }
    }

    private bool CheckDistance(Vector3 playerPos, Vector3 targetPos, float stoppingDistance)
    {
        playerPos.y = 0;
        targetPos.y = 0;

        if (Vector3.Distance(playerPos, targetPos) < stoppingDistance)
        {
            return true;
        }

        return false;
    }
}

