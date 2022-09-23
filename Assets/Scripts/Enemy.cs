using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SkillScript;

public class Enemy : MonoBehaviour
{
    [SerializeField] private new string name = "Dickhead";
    [SerializeField] private int moveSpeed = 3;
    [SerializeField] private int rangeAttack = 1;
    [SerializeField] private int damage = 20;
    [SerializeField] private int armorPercentPenetration = 20;
    [SerializeField] private int magresPercentPenetration = 0;
    [SerializeField] private int health = 50;
    [SerializeField] private float armor = 0;
    [SerializeField] private int magresist = 0;

    private GameObject cellsParent;

    EnemyClass enemyUnit;
    GameManager gameManager;

    private List<GameObject> cellsMoveList = new List<GameObject>();

    // Start is called before the first frame update
    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        cellsParent = GameObject.Find("cellsParent");

        HealthBar healthBar = transform.GetChild(0).GetChild(0).GetComponent<HealthBar>();

        enemyUnit = new EnemyClass(name,
                                 gameObject,
                                 rangeAttack,
                                 moveSpeed,
                                 damage,
                                 armorPercentPenetration,
                                 magresPercentPenetration,
                                 health,
                                 armor,
                                 magresist,
                                 healthBar);

        Skill skill = new Skill("sword", 10, 1, TypeDamage.Physical, 0, null);
        enemyUnit.AddSkill(skill, 0);
        skill = new Skill("fireball", 15, 3, TypeDamage.Magick, 2, null);
        enemyUnit.AddSkill(skill, 1);

        //Setup initial health in healthbar
        healthBar.NumberInHealthBar(health);

        Global.persons.Add(enemyUnit);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //find the nearest player
    void LookingForNearbestPlayer()
    {
        int xPlayer = 99;
        int yPlayer = 99;
        Person.PersonClass person = null;

        //get coordinates cell which enemy stay it
        string[] xy = transform.parent.name.Split(new char[] { ' ' });
        int xEnemy = int.Parse(xy[1]);
        int yEnemy = int.Parse(xy[0]);

        for (int i = 0; i < Global.persons.Count; i++)
        {
            if (Global.persons[i].ObjectPerson.tag == "Player")
            {
                //get coordinates cell which player stay it
                string[] xy_ = Global.persons[i].ObjectPerson.transform.parent.name.Split(new char[] { ' ' });
                int xPlayer_ = int.Parse(xy_[1]);
                int yPlayer_ = int.Parse(xy_[0]);

                //Checking which player closer
                if (Mathf.Abs(xPlayer_ - xEnemy) + Mathf.Abs(yPlayer_ - yEnemy)
                    < Mathf.Abs(xPlayer - xEnemy) + Mathf.Abs(yPlayer - yEnemy))
                {
                    xPlayer = xPlayer_;
                    yPlayer = yPlayer_;
                    person = Global.persons[i];
                }
            }
        }

        MoveEnemy(xPlayer, yPlayer, person);
    }

    //Cell in list which enemy can move
    public void CellInMoveList()
    {
        //clear list of cells
        CleanCells();

        //get coordinates cell which player stay it
        string[] xy = transform.parent.name.Split(new char[] { ' ' });
        int x = int.Parse(xy[1]);
        int y = int.Parse(xy[0]);

        //Add the cell on which the player is located
        cellsMoveList.Add(transform.parent.gameObject);

        //Breadth-first search
        //count of steps for moving to cell
        for (int step = 1; step <= enemyUnit.MoveSpeed; step++)
        {
            int countCells = cellsMoveList.Count;
            for (int i = 0; i < countCells; i++)
            {
                xy = cellsMoveList[i].name.Split(new char[] { ' ' });
                x = int.Parse(xy[1]);
                y = int.Parse(xy[0]);

                for (int j = 0; j < cellsParent.transform.childCount; j++)
                {
                    GameObject cell = cellsParent.transform.GetChild(j).gameObject;

                    bool skip = false;
                    //skip iteration if on this cell stay player
                    for (int ch = 0; ch < cell.transform.childCount; ch++)
                    {
                        if (cell.transform.GetChild(ch).tag == "Player") skip = true;
                    }

                    //skip iteration if this cell already in the list
                    for (int c = 0; c < cellsMoveList.Count; c++)
                    {
                        if (cell == cellsMoveList[c]) skip = true;
                    }

                    if (skip) continue;

                    string[] xy_ = cell.name.Split(new char[] { ' ' });
                    int x_ = int.Parse(xy_[1]);
                    int y_ = int.Parse(xy_[0]);

                    if ((Mathf.Abs(x - x_) + Mathf.Abs(y - y_)) == 1)
                    {
                        cellsMoveList.Add(cell);
                    }
                }
            }
        }
        //Remove the cell on which enemy stay
        cellsMoveList.RemoveAt(0);
        LookingForNearbestPlayer();
    }

    //move enemy to the nearest player 
    void MoveEnemy(int xPlayer, int yPlayer, Person.PersonClass player)
    {
        //get coordinates cell which player stay it
        string[] xy = transform.parent.name.Split(new char[] { ' ' });
        int xEnemy = int.Parse(xy[1]);
        int yEnemy = int.Parse(xy[0]);

        int range = 99;

        //if player in range one cell, don`t move
        if (Mathf.Abs(xPlayer - xEnemy) + Mathf.Abs(yPlayer - yEnemy) == 1)
        {
            range = 1;
        }

        //find the nearest cell to the nearest player
        else
        {
            GameObject cellToMove = transform.parent.gameObject;

            string[] xyCell = cellToMove.name.Split(new char[] { ' ' });
            int xCell = int.Parse(xyCell[1]);
            int yCell = int.Parse(xyCell[0]);

            for (int i = 0; i < cellsMoveList.Count; i++)
            {
                string[] xyCell_ = cellsMoveList[i].name.Split(new char[] { ' ' });
                int xCell_ = int.Parse(xyCell_[1]);
                int yCell_ = int.Parse(xyCell_[0]);

                if (Mathf.Abs(xPlayer - xCell_) + Mathf.Abs(yPlayer - yCell_)
                    < Mathf.Abs(xPlayer - xCell) + Mathf.Abs(yPlayer - yCell))
                {
                    cellToMove = cellsMoveList[i];
                    xCell = xCell_;
                    yCell = yCell_;
                    range = Mathf.Abs(xPlayer - xCell_) + Mathf.Abs(yPlayer - yCell_);
                }
            }

            //Move
            transform.position = cellToMove.transform.position;
            transform.parent = cellToMove.transform;
        }

        Attack(player, range);
    }

    //Choose spell and attack player
    void Attack(Person.PersonClass player, int range)
    {
        int maxDamage = 0;
        enemyUnit.Skill = null;

        //Choose cell which is not in cd, player in spell`s range, and spell gives max damage 
        for (int i = 0; i < enemyUnit.arrSkill.Length; i++)
        {
            if (enemyUnit.arrSkill[i] is not null
                && enemyUnit.arrSkill[i].Ready
                && enemyUnit.arrSkill[i].Range >= range
                && enemyUnit.arrSkill[i].Damage > maxDamage)
            {
                maxDamage = enemyUnit.arrSkill[i].Damage;
                enemyUnit.Skill = enemyUnit.arrSkill[i];
            }
        }

        enemyUnit.Attack(player);
    }

    void CleanCells()
    {
        cellsMoveList.Clear();
    }

    class EnemyClass : Person.PersonClass
    {
        public EnemyClass(string name,
                           GameObject objectPerson,
                           int rangeAttack,
                           int moveSpeed,
                           int damage,
                           float armorPercentPenetration,
                           float magresPercentPenetration,
                           int healtMAX,
                           float armor,
                           float magresist,
                           HealthBar healthBar) : base(name,
                                                   objectPerson,
                                                   rangeAttack,
                                                   moveSpeed,
                                                   damage,
                                                   armorPercentPenetration,
                                                   magresPercentPenetration,
                                                   healtMAX,
                                                   armor,
                                                   magresist,
                                                   healthBar)
        {
        }

        //the actions of the enemy when his turn begins
        public override void StartRound()
        {
            Debug.Log("Enemy turn");
            ObjectPerson.GetComponent<Enemy>().CellInMoveList();
            DecreaseCooldown();
        }

        public override void Attack(Person.PersonClass person)
        {
            if (Skill is not null)
            {
                person.TakeDamage(Skill.Damage, Skill.TypeDamage, ArmorPercentPenetration, MagresPercentPenetration);
                Skill.GoToCooldown();
                Debug.Log(Skill.Name);
            }
            
            ObjectPerson.GetComponent<Enemy>().gameManager.ChangeTurn();
        }

        protected override void Death()
        {
            Destroy(ObjectPerson);
        }
    }
}
