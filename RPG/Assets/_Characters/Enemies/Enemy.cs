﻿using RPG.Characters;
using RPG.Core;
using RPG.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityStandardAssets.Characters.ThirdPerson;

namespace RPG.Characters
{
    public class Enemy : MonoBehaviour, IDamageable
    {

        [SerializeField] float maxHealth = 100f;
        [SerializeField] WeaponConfig weaponInUse;
        [SerializeField] AnimatorOverrideController animatorOverrideController;
        [SerializeField] float attackRange = 12f;
        
        WeaponType type;
        Player player;
        WeaponConfig playerWeapon;
        AICharacterControl aiCharacterControl;
        Weapon weapon;
        Animator animator;
        NavMeshAgent navMesh;
        GameObject weaponPrefab;
        GameObject dominantHand;

        float currentHealth;
        float timeBetweenAttacks;
        float timeBetweenHits;
        float hitTimer;
        bool isAlive;
        public float healthAsPercentage
        {
            get
            {
                float healthAsPercentage = currentHealth / maxHealth;
                return healthAsPercentage;
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            animator = GetComponent<Animator>();
            navMesh = GetComponent<NavMeshAgent>();
            aiCharacterControl = GetComponent<AICharacterControl>();
            SetupWeapon();
            SetMaxHealth();
            FindPlayer();
            FindCurrentEquipedPlayerWeapon();
            SetupAnimatorOverriderController();
            
            PutWeaponInHands();
        }

        private void SetupWeapon()
        {
            weaponPrefab = weaponInUse.GetWeaponModel;
            type = weaponInUse.type;
            weapon = weaponPrefab.GetComponent<Weapon>();
            timeBetweenAttacks = weaponInUse.GetTimeBetweenAttacks;
        }

        private void PutWeaponInHands()
        {
            weaponPrefab = weaponInUse.GetWeaponModel;
            if (weaponInUse != null && weaponPrefab != null)
            {
                dominantHand = RequestDominantHand();
                var weapon = Instantiate(weaponPrefab, dominantHand.transform);
                weapon.transform.localPosition = weaponInUse.weaponGrip.localPosition;
                weapon.transform.localRotation = weaponInUse.weaponGrip.localRotation;

                // TODO: Change to bow only
                if (type == WeaponType.Ranged)
                {
                    GameObject arrowSpawnPoint = weaponInUse.GetProjectilePrefab.transform.Find("ArrowSpawnPoint").gameObject;
                    Instantiate(weaponInUse.GetProjectilePrefab, arrowSpawnPoint.transform.position, Quaternion.identity);
                }

            }
        }

        private GameObject RequestDominantHand()
        {
            var dominantHands = GetComponentsInChildren<DominantHand>();
            int numberOfDominantHands = dominantHands.Length;
            Assert.IsFalse(numberOfDominantHands <= 0, "Could not find any dominant hands, please add one");
            Assert.IsFalse(numberOfDominantHands > 1, "Found multiple dominant hands, please remove one");
            return dominantHands[0].gameObject;
        }

        private void SetupAnimatorOverriderController()
        {
            animator = GetComponent<Animator>();
            animator.runtimeAnimatorController = animatorOverrideController;
            animatorOverrideController["Basic Attack"] = weaponInUse.GetAttackAnimation;
            animatorOverrideController["Idle"] = weaponInUse.GetIdleAnimation;
            animatorOverrideController["Death"] = weaponInUse.GetDeathAnimation;
            animatorOverrideController["Run"] = weaponInUse.GetRunningAnimation;
        }

        private void FindCurrentEquipedPlayerWeapon()
        {
            playerWeapon = player.GetPlayerWeapon;
            timeBetweenHits = playerWeapon.GetTimeBetweenAttacks;
        }

        private void FindPlayer()
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            player = playerObject.GetComponent<Player>();
        }

        private void SetMaxHealth()
        {
            currentHealth = maxHealth;
        }

        // Update is called once per frame
        void Update()
        {
            CheckDistanceToPlayer();
        }

        private void CheckDistanceToPlayer()
        {
            float dist = Vector3.Distance(transform.position, player.transform.position);
            if (dist <= attackRange)
            {
                aiCharacterControl.SetTarget(player.transform);
            }
            else
                aiCharacterControl.SetTarget(null); 
        }

        public void TakeDamage(float damage)
        {
            currentHealth = currentHealth - damage;

            if (currentHealth <= 0)
            {
                Die();                
            }
                
        }

        private void Die()
        {
            navMesh.enabled = false;
            animator.enabled = false;
            GameObject socket = transform.Find("UI Socket").gameObject;
            socket.SetActive(false);
            aiCharacterControl.enabled = false;
        }
    }
}

