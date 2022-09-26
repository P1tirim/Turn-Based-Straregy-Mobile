using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SkillScript;

public class Person : MonoBehaviour
{
    public abstract class PersonClass
    {
        public string Name { get; protected set; }
        public GameObject ObjectPerson { get; protected set; }
        public int MoveSpeed { get; private set; }
        public Skill Skill { get; set; }
        public float ArmorPercentPenetration { get; private set; }
        public float MagresPercentPenetration { get; private set; }
        public float HealthMAX { get; private set; }
        public float HealthCurrent { get; protected set; }
        public float Armor { get; private set; }
        public float Magresit { get; private set; }
        public HealthBar HealthBar { get; private set; }

        public Skill[] arrSkill = new Skill[4];
        public bool haveMove = false;

        public PersonClass(string name, GameObject objectPerson, int rangeAttack, int moveSpeed, int damage, float armorPercentPenetration, float magresPercentPenetration, int healthMAX, float armor, float magresit, HealthBar healthBar)
        {
            Name = name;
            ObjectPerson = objectPerson;
            MoveSpeed = moveSpeed;
            ArmorPercentPenetration = armorPercentPenetration;
            MagresPercentPenetration = magresPercentPenetration;
            HealthMAX = healthMAX;
            HealthCurrent = healthMAX;
            Armor = armor;
            Magresit = magresit;
            HealthBar = healthBar;
        }

        public abstract void StartRound();

        //Replace skill in array of skills which player own
        public void AddSkill(Skill spell, int index)
        {
            arrSkill[index] = spell;
        }

        //the damage decreases depending on the protective indicators
        public void TakeDamage(int damage)
        {
            HealthCurrent -= damage;
            Debug.Log($"{ObjectPerson.name} {HealthCurrent}");

            HealthBar.UpdateHealthBar(HealthCurrent, HealthMAX);

            if (HealthCurrent <= 0) Death();
        }

        //Destroy object
        protected abstract void Death();

        public abstract void Attack(PersonClass person);


        //When round start cd all of skills decreases by 1
        protected void DecreaseCooldown()
        {
            for (int i = 0; i < arrSkill.Length; i++)
            {
                if (arrSkill[i] is not null) arrSkill[i].DecreaseCurrentCooldown();
            }
        }
    }
}
