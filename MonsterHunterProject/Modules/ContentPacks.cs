using RoR2.ContentManagement;

namespace MonsterHunter.Modules
{
    internal class ContentPacks : IContentPackProvider
    {
        internal ContentPack contentPack = new ContentPack();
        public string identifier => MainPlugin.MODUID;

        public void Initialize()
        {
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public System.Collections.IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            this.contentPack.identifier = this.identifier;
            contentPack.bodyPrefabs.Add(Prefabs.bodyPrefabs.ToArray());
            contentPack.buffDefs.Add(Buffs.buffDefs.ToArray());
            contentPack.effectDefs.Add(MonsterHunterAssets.effectDefs.ToArray());
            contentPack.entityStateTypes.Add(States.entityStates.ToArray());
            contentPack.itemDefs.Add(MonsterHunterAssets.itemDefs.ToArray());
            //contentPack.itemDefs.Add(DanteWeaponCatalog.itemDefs.ToArray());
            /*contentPack.itemTierDefs.Add(new RoR2.ItemTierDef[]
                {
                    Content.Survivors.MonsterHunter.devilArmTier
                });*/
            contentPack.masterPrefabs.Add(Prefabs.masterPrefabs.ToArray());
            contentPack.networkSoundEventDefs.Add(MonsterHunterAssets.networkSoundEventDefs.ToArray());
            contentPack.projectilePrefabs.Add(Prefabs.projectilePrefabs.ToArray());
            contentPack.skillDefs.Add(Skills.skillDefs.ToArray());
            contentPack.skillFamilies.Add(Skills.skillFamilies.ToArray());
            contentPack.survivorDefs.Add(Prefabs.survivorDefinitions.ToArray());
            contentPack.unlockableDefs.Add(Content.Survivors.MonsterHunter.unlockableDefs.ToArray());

            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(this.contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}