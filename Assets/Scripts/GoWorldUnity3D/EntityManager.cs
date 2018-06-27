using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GoWorldUnity3D
{
    public class EntityManager
    {
        internal static EntityManager Instance = new EntityManager();

        const string SPACE_ENTITY_NAME = "__space__";

        Dictionary<string, ClientEntity> entities = new Dictionary<string, ClientEntity>();
        Dictionary<string, GameObject> entityGameObjects = new Dictionary<string, GameObject>();
        public ClientEntity ClientOwner;
        public ClientSpace Space;

        internal ClientEntity CreateEntity(string typeName, string entityID, bool isClientOwner, float x, float y, float z, float yaw, MapAttr attrs)
        {
            if (typeName == SPACE_ENTITY_NAME)
            {
                typeName = "ClientSpace";
            }

            GoWorldLogger.Assert(this.entityGameObjects.ContainsKey(typeName));

            if (this.entities.ContainsKey(entityID))
            {
                ClientEntity old = this.entities[entityID];
                GoWorldLogger.Warn("EntityManager", "Entity {0} Already Exists, Destroying Old One: {1}", entityID, old);
                old.Destroy();
            }

            // create new Game Object of specified type 
            GameObject gameObjectPrefab = this.entityGameObjects[typeName];
            GameObject gameObject = GameObject.Instantiate(gameObjectPrefab);
            GameObject.DontDestroyOnLoad(gameObject);
            ClientEntity e = gameObject.GetComponent<ClientEntity>();
            GoWorldLogger.Assert(e.GetType().Name == typeName);
            e.init(entityID, isClientOwner, x, y, z, yaw, attrs);
            this.entities[entityID] = e;
            e.onCreated();

            // new entity created 
            if (e.IsSpace)
            {
                // enter new space
                if (this.Space != null)
                {
                    this.Space.Destroy();
                }

                this.Space = e as ClientSpace;
                this.onEnterSpace();
            } else
            {
                if (e.IsClientOwner)
                {
                    if (this.ClientOwner != null)
                    {
                        GoWorldLogger.Warn("EntityManager", "Replacing Existing Player: " + this.ClientOwner);
                        this.ClientOwner.Destroy();
                    }

                    this.ClientOwner = e;
                    e.becomeClientOwner();
                }

                if (this.Space != null)
                {
                    e.enterSpace();
                }
            }
            return e; 
        }

        internal void Update()
        {
            if (this.ClientOwner != null)
            {
                this.ClientOwner.syncPositionYawFromClient();
            }
        }

        internal ClientEntity getEntity(string entityID)
        {
            try
            {
                return this.entities[entityID]; 
            } catch (KeyNotFoundException)
            {
                return null;
            }
        }

        private void onEnterSpace()
        {
            foreach (ClientEntity entity in this.entities.Values)
            {
                if (!entity.IsSpace)
                {
                    entity.enterSpace();
                }
            }
        }

        private void onLeaveSpace()
        {
            foreach (ClientEntity entity in this.entities.Values)
            {
                if (!entity.IsSpace)
                {
                    entity.leaveSpace();
                }
            }
        }


        internal void delEntity(ClientEntity e)
        {
            this.entities.Remove(e.ID);
            if (this.Space == e)
            {
                this.Space = null;
                this.onLeaveSpace();
            } else
            {
                if (this.ClientOwner == e)
                {
                    this.ClientOwner = null;
                }

                if (this.Space != null)
                {
                    e.leaveSpace();
                }
            }
        }

        internal void RegisterEntity(GameObject gameObject)
        {
            ClientEntity entity = gameObject.GetComponent<ClientEntity>();
            UnityEngine.Debug.Assert(entity != null);
            // Debug.Assert(entityType.IsSubclassOf(typeof(ClientEntity)));
            Type entityType = entity.GetType();

            string entityTypeName = entityType.Name;
            GoWorldLogger.Assert(!this.entityGameObjects.ContainsKey(entityTypeName));
            this.entityGameObjects[entityTypeName] = gameObject;
        }

        internal void CallEntity(string entityID, string method, object[] args)
        {
            ClientEntity entity; 
            if (this.entities.TryGetValue(entityID, out entity))
            {
                System.Reflection.MethodInfo methodInfo = entity.GetType().GetMethod(method);
                if (methodInfo == null)
                {
                    GoWorldLogger.Error("EntityManager", "Call Entity {0}.{1}({2} Args) Failed: Public Method Not Found", entity, method, args.Length);
                    return;
                }

                GoWorldLogger.Debug("EntityManager", "Call Entity {0}: {1}({2} Args)", entity, method, args.Length);
                methodInfo.Invoke(entity, args);
            } else
            {
                // entity not found
                GoWorldLogger.Error("EntityManager", "Call Entity {0}.{1}({2} Args) Failed: Entity Not Found", entityID, method, args.Length);
            }
        }

        internal void DestroyEntity(string entityID)
        {
            GoWorldLogger.Debug("EntityManager", "Destroy Entity {0}", entityID);
            ClientEntity entity;
            if (this.entities.TryGetValue(entityID, out entity)) {
                entity.Destroy();
            } else
            {
                GoWorldLogger.Error("EntityManager", "Destroy Entity {0} Failed: Entity Not Found", entityID);
            }
        }

        internal void OnSyncEntityInfo(string entityID, float x, float y, float z, float yaw)
        {
            ClientEntity entity;
            try
            {
                entity = this.entities[entityID];
            } catch (KeyNotFoundException)
            {
                GoWorldLogger.Warn("EntityManager", "Entity {0} Sync Entity Info Failed: Entity Not Found", entityID);
                return;
            }

            entity.OnSyncEntityInfo(x, y, z, yaw);
        }

        internal void OnMapAttrChange(string entityID, ListAttr path, string key, object val)
        {
            ClientEntity entity;
            try
            {
                entity = this.entities[entityID];
            }
            catch (KeyNotFoundException)
            {
                GoWorldLogger.Warn("EntityManager", "Entity {0} Sync Entity Info Failed: Entity Not Found", entityID);
                return;
            }

            entity.OnMapAttrChange(path, key, val);
        }

        internal void OnMapAttrDel(string entityID, ListAttr path, string key)
        {
            ClientEntity entity;
            try
            {
                entity = this.entities[entityID];
            }
            catch (KeyNotFoundException)
            {
                GoWorldLogger.Warn("EntityManager", "Entity {0} Sync Entity Info Failed: Entity Not Found", entityID);
                return;
            }

            entity.OnMapAttrDel(path, key);
        }

        internal void OnMapAttrClear(string entityID, ListAttr path)
        {
            ClientEntity entity;
            try
            {
                entity = this.entities[entityID];
            }
            catch (KeyNotFoundException)
            {
                GoWorldLogger.Warn("EntityManager", "Entity {0} Sync Entity Info Failed: Entity Not Found", entityID);
                return;
            }

            entity.OnMapAttrClear(path);
        }
    }


}
