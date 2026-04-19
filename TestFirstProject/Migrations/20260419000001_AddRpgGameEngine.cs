using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable enable

namespace TestFirstProject.Migrations
{
    /// <summary>
    /// Creates all tables for the RPG game engine: characters, skills, battles,
    /// items, inventory, enemies, loot, zones, dungeons, and quests.
    /// Also seeds initial game data.
    /// </summary>
    public partial class AddRpgGameEngine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Zones ---
            migrationBuilder.CreateTable(
                name: "game_zones",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    min_level = table.Column<int>(type: "integer", nullable: false),
                    max_level = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_zones", x => x.id);
                });

            migrationBuilder.CreateIndex(name: "IX_game_zones_name", table: "game_zones", column: "name", unique: true);

            // --- Characters ---
            migrationBuilder.CreateTable(
                name: "game_characters",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    @class = table.Column<string>(name: "class", type: "character varying(20)", maxLength: 20, nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    experience = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    skill_points = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    current_hp = table.Column<int>(type: "integer", nullable: false),
                    current_mp = table.Column<int>(type: "integer", nullable: false),
                    gold = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_characters", x => x.id);
                });

            migrationBuilder.CreateIndex(name: "IX_game_characters_player_id", table: "game_characters", column: "player_id");
            migrationBuilder.CreateIndex(name: "IX_game_characters_player_id_name", table: "game_characters", columns: new[] { "player_id", "name" }, unique: true);

            // --- Character Stats ---
            migrationBuilder.CreateTable(
                name: "game_character_stats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_hp = table.Column<int>(type: "integer", nullable: false),
                    base_mp = table.Column<int>(type: "integer", nullable: false),
                    base_attack = table.Column<int>(type: "integer", nullable: false),
                    base_defense = table.Column<int>(type: "integer", nullable: false),
                    base_magic_power = table.Column<int>(type: "integer", nullable: false),
                    base_speed = table.Column<int>(type: "integer", nullable: false),
                    base_crit_chance = table.Column<double>(type: "double precision", nullable: false),
                    base_dodge_chance = table.Column<double>(type: "double precision", nullable: false),
                    bonus_hp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    bonus_mp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    bonus_attack = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    bonus_defense = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    bonus_magic_power = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    bonus_speed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_character_stats", x => x.id);
                    table.ForeignKey(name: "FK_game_character_stats_game_characters_character_id", column: x => x.character_id, principalTable: "game_characters", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_game_character_stats_character_id", table: "game_character_stats", column: "character_id", unique: true);

            // --- Skills ---
            migrationBuilder.CreateTable(
                name: "game_skills",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    @class = table.Column<string>(name: "class", type: "character varying(20)", maxLength: 20, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    unlock_level = table.Column<int>(type: "integer", nullable: false),
                    mp_cost = table.Column<int>(type: "integer", nullable: false),
                    cooldown_turns = table.Column<int>(type: "integer", nullable: false),
                    target_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    damage_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    base_damage = table.Column<int>(type: "integer", nullable: false),
                    damage_multiplier = table.Column<double>(type: "double precision", nullable: false, defaultValue: 1.0),
                    passive_hp_bonus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    passive_mp_bonus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    passive_attack_bonus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    passive_defense_bonus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    passive_magic_power_bonus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    passive_speed_bonus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    applies_effect = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    effect_duration = table.Column<int>(type: "integer", nullable: false),
                    effect_tick_value = table.Column<int>(type: "integer", nullable: false),
                    effect_stack_limit = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_skills", x => x.id);
                });

            migrationBuilder.CreateIndex(name: "IX_game_skills_class", table: "game_skills", column: "class");
            migrationBuilder.CreateIndex(name: "IX_game_skills_class_unlock_level", table: "game_skills", columns: new[] { "class", "unlock_level" });

            // --- Learned Skills ---
            migrationBuilder.CreateTable(
                name: "game_learned_skills",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_id = table.Column<Guid>(type: "uuid", nullable: false),
                    skill_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_cooldown = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_learned_skills", x => x.id);
                    table.ForeignKey(name: "FK_game_learned_skills_game_characters_character_id", column: x => x.character_id, principalTable: "game_characters", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_game_learned_skills_game_skills_skill_id", column: x => x.skill_id, principalTable: "game_skills", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_game_learned_skills_character_id_skill_id", table: "game_learned_skills", columns: new[] { "character_id", "skill_id" }, unique: true);

            // --- Items ---
            migrationBuilder.CreateTable(
                name: "game_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    rarity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    level_requirement = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    class_restriction = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    bonus_hp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    bonus_mp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    bonus_attack = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    bonus_defense = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    bonus_magic_power = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    bonus_speed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    bonus_crit_chance = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    bonus_dodge_chance = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    consumable_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    heal_amount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    mana_amount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    buff_effect = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    buff_duration = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    buy_price = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    sell_price = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_items", x => x.id);
                });

            migrationBuilder.CreateIndex(name: "IX_game_items_type", table: "game_items", column: "type");
            migrationBuilder.CreateIndex(name: "IX_game_items_rarity", table: "game_items", column: "rarity");

            // --- Inventory Items ---
            migrationBuilder.CreateTable(
                name: "game_inventory_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    enchantment_level = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    enchant_bonus_attack = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    enchant_bonus_defense = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    enchant_bonus_hp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    enchant_bonus_magic_power = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_inventory_items", x => x.id);
                    table.ForeignKey(name: "FK_game_inventory_items_game_characters_character_id", column: x => x.character_id, principalTable: "game_characters", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_game_inventory_items_game_items_item_id", column: x => x.item_id, principalTable: "game_items", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_game_inventory_items_character_id", table: "game_inventory_items", column: "character_id");
            migrationBuilder.CreateIndex(name: "IX_game_inventory_items_character_id_item_id", table: "game_inventory_items", columns: new[] { "character_id", "item_id" });

            // --- Equipped Items ---
            migrationBuilder.CreateTable(
                name: "game_equipped_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inventory_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    slot = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_equipped_items", x => x.id);
                    table.ForeignKey(name: "FK_game_equipped_items_game_characters_character_id", column: x => x.character_id, principalTable: "game_characters", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_game_equipped_items_game_inventory_items_inventory_item_id", column: x => x.inventory_item_id, principalTable: "game_inventory_items", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_game_equipped_items_character_id_slot", table: "game_equipped_items", columns: new[] { "character_id", "slot" }, unique: true);

            // --- Stash ---
            migrationBuilder.CreateTable(
                name: "game_stash",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_stash", x => x.id);
                    table.ForeignKey(name: "FK_game_stash_game_items_item_id", column: x => x.item_id, principalTable: "game_items", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_game_stash_player_id", table: "game_stash", column: "player_id");
            migrationBuilder.CreateIndex(name: "IX_game_stash_player_id_item_id", table: "game_stash", columns: new[] { "player_id", "item_id" });

            // --- Enemies ---
            migrationBuilder.CreateTable(
                name: "game_enemies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_boss = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    boss_mechanic = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    base_hp = table.Column<int>(type: "integer", nullable: false),
                    base_mp = table.Column<int>(type: "integer", nullable: false),
                    base_attack = table.Column<int>(type: "integer", nullable: false),
                    base_defense = table.Column<int>(type: "integer", nullable: false),
                    base_magic_power = table.Column<int>(type: "integer", nullable: false),
                    base_speed = table.Column<int>(type: "integer", nullable: false),
                    base_crit_chance = table.Column<double>(type: "double precision", nullable: false),
                    base_dodge_chance = table.Column<double>(type: "double precision", nullable: false),
                    primary_damage_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    base_xp_reward = table.Column<int>(type: "integer", nullable: false),
                    base_gold_reward = table.Column<int>(type: "integer", nullable: false),
                    zone_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_enemies", x => x.id);
                    table.ForeignKey(name: "FK_game_enemies_game_zones_zone_id", column: x => x.zone_id, principalTable: "game_zones", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(name: "IX_game_enemies_zone_id", table: "game_enemies", column: "zone_id");
            migrationBuilder.CreateIndex(name: "IX_game_enemies_is_boss", table: "game_enemies", column: "is_boss");

            // --- Enemy Skills (join table) ---
            migrationBuilder.CreateTable(
                name: "game_enemy_skills",
                columns: table => new
                {
                    EnemyId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_enemy_skills", x => new { x.EnemyId, x.SkillsId });
                    table.ForeignKey(name: "FK_game_enemy_skills_game_enemies_EnemyId", column: x => x.EnemyId, principalTable: "game_enemies", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_game_enemy_skills_game_skills_SkillsId", column: x => x.SkillsId, principalTable: "game_skills", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_game_enemy_skills_SkillsId", table: "game_enemy_skills", column: "SkillsId");

            // --- Loot Table ---
            migrationBuilder.CreateTable(
                name: "game_loot_table",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    enemy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    drop_chance = table.Column<double>(type: "double precision", nullable: false),
                    min_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    max_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_loot_table", x => x.id);
                    table.ForeignKey(name: "FK_game_loot_table_game_enemies_enemy_id", column: x => x.enemy_id, principalTable: "game_enemies", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_game_loot_table_game_items_item_id", column: x => x.item_id, principalTable: "game_items", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_game_loot_table_enemy_id", table: "game_loot_table", column: "enemy_id");

            // --- Battles ---
            migrationBuilder.CreateTable(
                name: "game_battles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    current_turn = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    current_turn_index = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    dungeon_room_id = table.Column<Guid>(type: "uuid", nullable: true),
                    xp_reward = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    gold_reward = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    row_version = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_battles", x => x.id);
                    table.ForeignKey(name: "FK_game_battles_game_characters_character_id", column: x => x.character_id, principalTable: "game_characters", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_game_battles_character_id", table: "game_battles", column: "character_id");
            migrationBuilder.CreateIndex(name: "IX_game_battles_character_id_status", table: "game_battles", columns: new[] { "character_id", "status" });

            // --- Battle Participants ---
            migrationBuilder.CreateTable(
                name: "game_battle_participants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    battle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    character_id = table.Column<Guid>(type: "uuid", nullable: true),
                    enemy_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    current_hp = table.Column<int>(type: "integer", nullable: false),
                    max_hp = table.Column<int>(type: "integer", nullable: false),
                    current_mp = table.Column<int>(type: "integer", nullable: false),
                    max_mp = table.Column<int>(type: "integer", nullable: false),
                    attack = table.Column<int>(type: "integer", nullable: false),
                    defense = table.Column<int>(type: "integer", nullable: false),
                    magic_power = table.Column<int>(type: "integer", nullable: false),
                    speed = table.Column<int>(type: "integer", nullable: false),
                    crit_chance = table.Column<double>(type: "double precision", nullable: false),
                    dodge_chance = table.Column<double>(type: "double precision", nullable: false),
                    turn_order = table.Column<int>(type: "integer", nullable: false),
                    is_phase_two = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_battle_participants", x => x.id);
                    table.ForeignKey(name: "FK_game_battle_participants_game_battles_battle_id", column: x => x.battle_id, principalTable: "game_battles", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_game_battle_participants_game_characters_character_id", column: x => x.character_id, principalTable: "game_characters", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(name: "FK_game_battle_participants_game_enemies_enemy_id", column: x => x.enemy_id, principalTable: "game_enemies", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(name: "IX_game_battle_participants_battle_id", table: "game_battle_participants", column: "battle_id");

            // --- Active Status Effects ---
            migrationBuilder.CreateTable(
                name: "game_active_status_effects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    battle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_participant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    remaining_turns = table.Column<int>(type: "integer", nullable: false),
                    tick_value = table.Column<int>(type: "integer", nullable: false),
                    stacks = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_active_status_effects", x => x.id);
                    table.ForeignKey(name: "FK_game_active_status_effects_game_battles_battle_id", column: x => x.battle_id, principalTable: "game_battles", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_game_active_status_effects_game_battle_participants_target_participant_id", column: x => x.target_participant_id, principalTable: "game_battle_participants", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_game_active_status_effects_battle_id", table: "game_active_status_effects", column: "battle_id");
            migrationBuilder.CreateIndex(name: "IX_game_active_status_effects_target_participant_id", table: "game_active_status_effects", column: "target_participant_id");

            // --- Dungeon Runs ---
            migrationBuilder.CreateTable(
                name: "game_dungeon_runs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_id = table.Column<Guid>(type: "uuid", nullable: false),
                    zone_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    current_room_index = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_dungeon_runs", x => x.id);
                    table.ForeignKey(name: "FK_game_dungeon_runs_game_characters_character_id", column: x => x.character_id, principalTable: "game_characters", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_game_dungeon_runs_game_zones_zone_id", column: x => x.zone_id, principalTable: "game_zones", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_game_dungeon_runs_character_id", table: "game_dungeon_runs", column: "character_id");
            migrationBuilder.CreateIndex(name: "IX_game_dungeon_runs_character_id_status", table: "game_dungeon_runs", columns: new[] { "character_id", "status" });

            // --- Dungeon Rooms ---
            migrationBuilder.CreateTable(
                name: "game_dungeon_rooms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    dungeon_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_index = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    enemy_id = table.Column<Guid>(type: "uuid", nullable: true),
                    treasure_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    trap_damage = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    restore_percent = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    battle_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_dungeon_rooms", x => x.id);
                    table.ForeignKey(name: "FK_game_dungeon_rooms_game_dungeon_runs_dungeon_run_id", column: x => x.dungeon_run_id, principalTable: "game_dungeon_runs", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_game_dungeon_rooms_game_enemies_enemy_id", column: x => x.enemy_id, principalTable: "game_enemies", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(name: "FK_game_dungeon_rooms_game_items_treasure_item_id", column: x => x.treasure_item_id, principalTable: "game_items", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(name: "FK_game_dungeon_rooms_game_battles_battle_id", column: x => x.battle_id, principalTable: "game_battles", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(name: "IX_game_dungeon_rooms_dungeon_run_id", table: "game_dungeon_rooms", column: "dungeon_run_id");
            migrationBuilder.CreateIndex(name: "IX_game_dungeon_rooms_dungeon_run_id_room_index", table: "game_dungeon_rooms", columns: new[] { "dungeon_run_id", "room_index" }, unique: true);

            // --- Quests ---
            migrationBuilder.CreateTable(
                name: "game_quests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    required_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    target_enemy_id = table.Column<Guid>(type: "uuid", nullable: true),
                    target_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    target_level = table.Column<int>(type: "integer", nullable: true),
                    prerequisite_quest_id = table.Column<Guid>(type: "uuid", nullable: true),
                    min_level_requirement = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    xp_reward = table.Column<int>(type: "integer", nullable: false),
                    gold_reward = table.Column<int>(type: "integer", nullable: false),
                    reward_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reward_item_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    time_limit_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    zone_id = table.Column<Guid>(type: "uuid", nullable: true),
                    next_quest_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_quests", x => x.id);
                    table.ForeignKey(name: "FK_game_quests_game_quests_prerequisite_quest_id", column: x => x.prerequisite_quest_id, principalTable: "game_quests", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(name: "FK_game_quests_game_items_reward_item_id", column: x => x.reward_item_id, principalTable: "game_items", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(name: "FK_game_quests_game_zones_zone_id", column: x => x.zone_id, principalTable: "game_zones", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(name: "FK_game_quests_game_quests_next_quest_id", column: x => x.next_quest_id, principalTable: "game_quests", principalColumn: "id", onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(name: "IX_game_quests_zone_id", table: "game_quests", column: "zone_id");
            migrationBuilder.CreateIndex(name: "IX_game_quests_type", table: "game_quests", column: "type");

            // --- Character Quests ---
            migrationBuilder.CreateTable(
                name: "game_character_quests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    current_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    accepted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_character_quests", x => x.id);
                    table.ForeignKey(name: "FK_game_character_quests_game_characters_character_id", column: x => x.character_id, principalTable: "game_characters", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(name: "FK_game_character_quests_game_quests_quest_id", column: x => x.quest_id, principalTable: "game_quests", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_game_character_quests_character_id", table: "game_character_quests", column: "character_id");
            migrationBuilder.CreateIndex(name: "IX_game_character_quests_character_id_quest_id", table: "game_character_quests", columns: new[] { "character_id", "quest_id" }, unique: true);
            migrationBuilder.CreateIndex(name: "IX_game_character_quests_character_id_status", table: "game_character_quests", columns: new[] { "character_id", "status" });

            // --- Seed Data ---
            SeedZones(migrationBuilder);
            SeedSkills(migrationBuilder);
            SeedItems(migrationBuilder);
            SeedEnemies(migrationBuilder);
            SeedLootTable(migrationBuilder);
            SeedQuests(migrationBuilder);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "game_character_quests");
            migrationBuilder.DropTable(name: "game_dungeon_rooms");
            migrationBuilder.DropTable(name: "game_dungeon_runs");
            migrationBuilder.DropTable(name: "game_active_status_effects");
            migrationBuilder.DropTable(name: "game_battle_participants");
            migrationBuilder.DropTable(name: "game_battles");
            migrationBuilder.DropTable(name: "game_equipped_items");
            migrationBuilder.DropTable(name: "game_inventory_items");
            migrationBuilder.DropTable(name: "game_stash");
            migrationBuilder.DropTable(name: "game_loot_table");
            migrationBuilder.DropTable(name: "game_learned_skills");
            migrationBuilder.DropTable(name: "game_enemy_skills");
            migrationBuilder.DropTable(name: "game_quests");
            migrationBuilder.DropTable(name: "game_enemies");
            migrationBuilder.DropTable(name: "game_skills");
            migrationBuilder.DropTable(name: "game_items");
            migrationBuilder.DropTable(name: "game_characters");
            migrationBuilder.DropTable(name: "game_character_stats");
            migrationBuilder.DropTable(name: "game_zones");
        }

        #region Seed Data Methods

        private static void SeedZones(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(table: "game_zones", columns: new[] { "id", "name", "description", "min_level", "max_level" }, values: new object[,]
            {
                { new Guid("a0000001-0000-0000-0000-000000000001"), "Whispering Woods", "A dark forest teeming with goblins and wolves. Ideal for new adventurers.", 1, 10 },
                { new Guid("a0000001-0000-0000-0000-000000000002"), "Cursed Catacombs", "Ancient underground tunnels haunted by the undead.", 10, 25 },
                { new Guid("a0000001-0000-0000-0000-000000000003"), "Dragon's Peak", "A volcanic mountain where only the strongest dare to venture.", 25, 50 }
            });
        }

        private static void SeedSkills(MigrationBuilder migrationBuilder)
        {
            // Warrior skills
            migrationBuilder.InsertData(table: "game_skills", columns: new[] { "id", "name", "description", "class", "type", "unlock_level", "mp_cost", "cooldown_turns", "target_type", "damage_type", "base_damage", "damage_multiplier", "applies_effect", "effect_duration", "effect_tick_value", "effect_stack_limit", "passive_hp_bonus", "passive_mp_bonus", "passive_attack_bonus", "passive_defense_bonus", "passive_magic_power_bonus", "passive_speed_bonus" }, values: new object[,]
            {
                { new Guid("d0000001-0000-0000-0000-000000000001"), "Power Slash", "A mighty sword strike that deals heavy physical damage.", "Warrior", "Active", 1, 5, 0, "Single", "Physical", 15, 1.5, null, 0, 0, 1, 0, 0, 0, 0, 0, 0 },
                { new Guid("d0000001-0000-0000-0000-000000000002"), "Shield Bash", "Bash the enemy with your shield, dealing damage and stunning them.", "Warrior", "Active", 5, 12, 2, "Single", "Physical", 10, 1.0, "Stun", 1, 0, 1, 0, 0, 0, 0, 0, 0 },
                { new Guid("d0000001-0000-0000-0000-000000000003"), "Whirlwind", "Spin your weapon, striking all enemies.", "Warrior", "Active", 12, 20, 3, "All", "Physical", 20, 1.2, null, 0, 0, 1, 0, 0, 0, 0, 0, 0 },
                { new Guid("d0000001-0000-0000-0000-000000000004"), "Berserk", "Enter a rage, applying Haste to yourself.", "Warrior", "Active", 20, 15, 4, "Self", "Physical", 0, 0.0, "Haste", 3, 5, 1, 0, 0, 0, 0, 0, 0 },
                { new Guid("d0000001-0000-0000-0000-000000000005"), "Toughness", "Passive: Increases HP and Defense.", "Warrior", "Passive", 8, 0, 0, "Single", "Physical", 0, 1.0, null, 0, 0, 1, 50, 0, 0, 10, 0, 0 },
                // Mage skills
                { new Guid("d0000002-0000-0000-0000-000000000001"), "Fireball", "Hurl a ball of fire that burns the target.", "Mage", "Active", 1, 10, 0, "Single", "Fire", 20, 1.8, "Burn", 3, 5, 2, 0, 0, 0, 0, 0, 0 },
                { new Guid("d0000002-0000-0000-0000-000000000002"), "Ice Shard", "Launch a shard of ice that slows the target.", "Mage", "Active", 5, 8, 1, "Single", "Ice", 15, 1.5, "Slow", 2, 0, 1, 0, 0, 0, 0, 0, 0 },
                { new Guid("d0000002-0000-0000-0000-000000000003"), "Lightning Bolt", "Call down lightning on all enemies.", "Mage", "Active", 15, 25, 3, "All", "Lightning", 25, 1.6, null, 0, 0, 1, 0, 0, 0, 0, 0, 0 },
                { new Guid("d0000002-0000-0000-0000-000000000004"), "Mana Shield", "Create a magical shield around yourself.", "Mage", "Active", 10, 20, 4, "Self", "Ice", 0, 0.0, "Shield", 3, 30, 1, 0, 0, 0, 0, 0, 0 },
                { new Guid("d0000002-0000-0000-0000-000000000005"), "Arcane Intellect", "Passive: Increases MP and Magic Power.", "Mage", "Passive", 8, 0, 0, "Single", "Physical", 0, 1.0, null, 0, 0, 1, 0, 50, 0, 0, 15, 0 },
                // Rogue skills
                { new Guid("d0000003-0000-0000-0000-000000000001"), "Backstab", "Strike from the shadows for massive damage.", "Rogue", "Active", 1, 8, 0, "Single", "Physical", 25, 2.0, null, 0, 0, 1, 0, 0, 0, 0, 0, 0 },
                { new Guid("d0000003-0000-0000-0000-000000000002"), "Poison Blade", "Coat your blade in poison, dealing damage over time.", "Rogue", "Active", 5, 10, 1, "Single", "Poison", 10, 1.2, "Poison", 4, 8, 3, 0, 0, 0, 0, 0, 0 },
                { new Guid("d0000003-0000-0000-0000-000000000003"), "Shadow Strike", "A devastating strike that causes bleeding.", "Rogue", "Active", 15, 18, 2, "Single", "Physical", 30, 1.8, "Bleed", 3, 6, 2, 0, 0, 0, 0, 0, 0 },
                { new Guid("d0000003-0000-0000-0000-000000000004"), "Evasion", "Passive: Increases Dodge Chance and Speed.", "Rogue", "Passive", 8, 0, 0, "Single", "Physical", 0, 1.0, null, 0, 0, 1, 0, 0, 0, 0, 0, 10 },
                { new Guid("d0000003-0000-0000-0000-000000000005"), "Blade Mastery", "Passive: Increases Attack power.", "Rogue", "Passive", 20, 0, 0, "Single", "Physical", 0, 1.0, null, 0, 0, 1, 0, 0, 20, 0, 0, 0 },
                // Paladin skills
                { new Guid("d0000004-0000-0000-0000-000000000001"), "Holy Smite", "Strike with divine power.", "Paladin", "Active", 1, 8, 0, "Single", "Fire", 18, 1.4, null, 0, 0, 1, 0, 0, 0, 0, 0, 0 },
                { new Guid("d0000004-0000-0000-0000-000000000002"), "Holy Light", "Heal yourself with divine energy.", "Paladin", "Active", 5, 15, 2, "Self", "Fire", 0, 0.0, "Regen", 3, 15, 1, 0, 0, 0, 0, 0, 0 },
                { new Guid("d0000004-0000-0000-0000-000000000003"), "Divine Shield", "Envelop yourself in a protective barrier.", "Paladin", "Active", 12, 20, 4, "Self", "Fire", 0, 0.0, "Shield", 2, 50, 1, 0, 0, 0, 0, 0, 0 },
                { new Guid("d0000004-0000-0000-0000-000000000004"), "Consecration", "Burn all enemies with holy fire.", "Paladin", "Active", 18, 22, 3, "All", "Fire", 15, 1.3, "Burn", 2, 4, 1, 0, 0, 0, 0, 0, 0 },
                { new Guid("d0000004-0000-0000-0000-000000000005"), "Devotion", "Passive: Increases HP, Defense, and MP.", "Paladin", "Passive", 10, 0, 0, "Single", "Physical", 0, 1.0, null, 0, 0, 1, 30, 20, 0, 8, 0, 0 }
            });
        }

        private static void SeedItems(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(table: "game_items", columns: new[] { "id", "name", "description", "type", "rarity", "level_requirement", "class_restriction", "bonus_hp", "bonus_mp", "bonus_attack", "bonus_defense", "bonus_magic_power", "bonus_speed", "bonus_crit_chance", "bonus_dodge_chance", "consumable_type", "heal_amount", "mana_amount", "buff_effect", "buff_duration", "buy_price", "sell_price" }, values: new object[,]
            {
                // Weapons
                { new Guid("c0000001-0000-0000-0000-000000000001"), "Rusty Sword", "A worn blade, but still sharp enough.", "Weapon", "Common", 1, null, 0, 0, 5, 0, 0, 0, 0.0, 0.0, null, 0, 0, null, 0, 50, 15 },
                { new Guid("c0000001-0000-0000-0000-000000000002"), "Wooden Staff", "A simple staff for channeling magic.", "Weapon", "Common", 1, "Mage", 0, 0, 0, 0, 8, 0, 0.0, 0.0, null, 0, 0, null, 0, 50, 15 },
                { new Guid("c0000001-0000-0000-0000-000000000004"), "Iron Sword", "A sturdy iron blade forged by a skilled smith.", "Weapon", "Uncommon", 5, null, 0, 0, 12, 0, 0, 0, 0.0, 0.0, null, 0, 0, null, 0, 200, 60 },
                { new Guid("c0000001-0000-0000-0000-000000000005"), "Arcane Staff", "A staff imbued with arcane energy.", "Weapon", "Rare", 10, "Mage", 0, 20, 0, 0, 25, 0, 0.0, 0.0, null, 0, 0, null, 0, 800, 250 },
                { new Guid("c0000001-0000-0000-0000-000000000007"), "Shadow Dagger", "A dagger that strikes from the darkness.", "Weapon", "Rare", 12, "Rogue", 0, 0, 18, 0, 0, 0, 0.05, 0.0, null, 0, 0, null, 0, 900, 280 },
                { new Guid("c0000001-0000-0000-0000-000000000008"), "Holy Mace", "A mace blessed by the divine.", "Weapon", "Rare", 10, "Paladin", 0, 0, 15, 0, 10, 0, 0.0, 0.0, null, 0, 0, null, 0, 850, 260 },
                { new Guid("c0000001-0000-0000-0000-000000000009"), "Dragonslayer", "A legendary blade forged to slay dragons.", "Weapon", "Legendary", 30, "Warrior", 0, 0, 50, 0, 0, 0, 0.1, 0.0, null, 0, 0, null, 0, 10000, 3000 },
                // Armor
                { new Guid("c0000001-0000-0000-0000-000000000003"), "Leather Armor", "Basic protection from light blows.", "Armor", "Common", 1, null, 10, 0, 0, 5, 0, 0, 0.0, 0.0, null, 0, 0, null, 0, 60, 18 },
                { new Guid("c0000001-0000-0000-0000-000000000006"), "Steel Plate Armor", "Heavy armor providing excellent defense.", "Armor", "Uncommon", 8, null, 30, 0, 0, 15, 0, 0, 0.0, 0.0, null, 0, 0, null, 0, 400, 120 },
                // Accessory
                { new Guid("c0000001-0000-0000-0000-000000000010"), "Amulet of Power", "An amulet that radiates arcane energy.", "Accessory", "Epic", 15, null, 20, 0, 8, 0, 12, 0, 0.0, 0.0, null, 0, 0, null, 0, 2000, 600 },
                // Consumables
                { new Guid("c0000002-0000-0000-0000-000000000001"), "Health Potion", "Restores 50 HP.", "Consumable", "Common", 1, null, 0, 0, 0, 0, 0, 0, 0.0, 0.0, "HealthPotion", 50, 0, null, 0, 25, 8 },
                { new Guid("c0000002-0000-0000-0000-000000000002"), "Mana Potion", "Restores 30 MP.", "Consumable", "Common", 1, null, 0, 0, 0, 0, 0, 0, 0.0, 0.0, "ManaPotion", 0, 30, null, 0, 25, 8 },
                { new Guid("c0000002-0000-0000-0000-000000000003"), "Great Health Potion", "Restores 150 HP.", "Consumable", "Uncommon", 1, null, 0, 0, 0, 0, 0, 0, 0.0, 0.0, "HealthPotion", 150, 0, null, 0, 100, 30 },
                { new Guid("c0000002-0000-0000-0000-000000000004"), "Scroll of Strength", "Temporarily increases Attack.", "Consumable", "Rare", 1, null, 0, 0, 0, 0, 0, 0, 0.0, 0.0, "Scroll", 0, 0, "Haste", 5, 200, 60 },
                { new Guid("c0000002-0000-0000-0000-000000000005"), "Scroll of Haste", "Temporarily increases Speed.", "Consumable", "Rare", 1, null, 0, 0, 0, 0, 0, 0, 0.0, 0.0, "Scroll", 0, 0, "Haste", 5, 200, 60 },
                // Materials
                { new Guid("c0000003-0000-0000-0000-000000000001"), "Enchantment Stone", "A crystallized stone used to enchant equipment.", "Material", "Uncommon", 1, null, 0, 0, 0, 0, 0, 0, 0.0, 0.0, null, 0, 0, null, 0, 150, 50 },
                { new Guid("c0000003-0000-0000-0000-000000000002"), "Magic Dust", "Sparkling dust used in enchantments.", "Material", "Common", 1, null, 0, 0, 0, 0, 0, 0, 0.0, 0.0, null, 0, 0, null, 0, 50, 15 }
            });
        }

        private static void SeedEnemies(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(table: "game_enemies", columns: new[] { "id", "name", "is_boss", "boss_mechanic", "zone_id", "base_hp", "base_mp", "base_attack", "base_defense", "base_magic_power", "base_speed", "base_crit_chance", "base_dodge_chance", "primary_damage_type", "base_xp_reward", "base_gold_reward" }, values: new object[,]
            {
                // Zone 1
                { new Guid("b0000001-0000-0000-0000-000000000001"), "Goblin", false, "None", new Guid("a0000001-0000-0000-0000-000000000001"), 60, 10, 12, 5, 3, 8, 0.05, 0.08, "Physical", 20, 10 },
                { new Guid("b0000001-0000-0000-0000-000000000003"), "Dire Wolf", false, "None", new Guid("a0000001-0000-0000-0000-000000000001"), 45, 0, 15, 3, 0, 14, 0.1, 0.12, "Physical", 25, 8 },
                { new Guid("b0000002-0000-0000-0000-000000000001"), "Goblin King", true, "Summon", new Guid("a0000001-0000-0000-0000-000000000001"), 300, 50, 25, 15, 10, 10, 0.08, 0.05, "Physical", 150, 100 },
                // Zone 2
                { new Guid("b0000001-0000-0000-0000-000000000002"), "Skeleton Warrior", false, "None", new Guid("a0000001-0000-0000-0000-000000000002"), 100, 0, 22, 18, 0, 7, 0.06, 0.03, "Physical", 45, 25 },
                { new Guid("b0000001-0000-0000-0000-000000000005"), "Dark Mage", false, "None", new Guid("a0000001-0000-0000-0000-000000000002"), 70, 80, 8, 10, 30, 9, 0.07, 0.06, "Fire", 55, 35 },
                { new Guid("b0000002-0000-0000-0000-000000000002"), "Lich Lord", true, "ShieldPhase", new Guid("a0000001-0000-0000-0000-000000000002"), 600, 200, 20, 25, 45, 8, 0.1, 0.05, "Ice", 400, 300 },
                // Zone 3
                { new Guid("b0000001-0000-0000-0000-000000000004"), "Orc Warlord", false, "None", new Guid("a0000001-0000-0000-0000-000000000003"), 200, 20, 40, 30, 5, 10, 0.12, 0.04, "Physical", 100, 60 },
                { new Guid("b0000001-0000-0000-0000-000000000006"), "Stone Golem", false, "None", new Guid("a0000001-0000-0000-0000-000000000003"), 350, 0, 35, 50, 0, 4, 0.03, 0.0, "Physical", 120, 70 },
                { new Guid("b0000002-0000-0000-0000-000000000003"), "Ancient Dragon", true, "Enrage", new Guid("a0000001-0000-0000-0000-000000000003"), 1500, 300, 55, 40, 50, 12, 0.15, 0.08, "Fire", 1000, 800 }
            });
        }

        private static void SeedLootTable(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(table: "game_loot_table", columns: new[] { "id", "enemy_id", "item_id", "drop_chance", "min_quantity", "max_quantity" }, values: new object[,]
            {
                { new Guid("f0000001-0000-0000-0000-000000000001"), new Guid("b0000001-0000-0000-0000-000000000001"), new Guid("c0000002-0000-0000-0000-000000000001"), 30.0, 1, 2 },
                { new Guid("f0000001-0000-0000-0000-000000000002"), new Guid("b0000001-0000-0000-0000-000000000001"), new Guid("c0000001-0000-0000-0000-000000000001"), 10.0, 1, 1 },
                { new Guid("f0000001-0000-0000-0000-000000000003"), new Guid("b0000001-0000-0000-0000-000000000003"), new Guid("c0000001-0000-0000-0000-000000000003"), 15.0, 1, 1 },
                { new Guid("f0000001-0000-0000-0000-000000000004"), new Guid("b0000001-0000-0000-0000-000000000003"), new Guid("c0000003-0000-0000-0000-000000000002"), 25.0, 1, 3 },
                { new Guid("f0000001-0000-0000-0000-000000000005"), new Guid("b0000002-0000-0000-0000-000000000001"), new Guid("c0000001-0000-0000-0000-000000000004"), 40.0, 1, 1 },
                { new Guid("f0000001-0000-0000-0000-000000000006"), new Guid("b0000002-0000-0000-0000-000000000001"), new Guid("c0000003-0000-0000-0000-000000000001"), 50.0, 1, 2 },
                { new Guid("f0000001-0000-0000-0000-000000000007"), new Guid("b0000001-0000-0000-0000-000000000002"), new Guid("c0000001-0000-0000-0000-000000000006"), 8.0, 1, 1 },
                { new Guid("f0000001-0000-0000-0000-000000000008"), new Guid("b0000001-0000-0000-0000-000000000002"), new Guid("c0000003-0000-0000-0000-000000000001"), 15.0, 1, 1 },
                { new Guid("f0000001-0000-0000-0000-000000000009"), new Guid("b0000001-0000-0000-0000-000000000005"), new Guid("c0000002-0000-0000-0000-000000000002"), 35.0, 1, 2 },
                { new Guid("f0000001-0000-0000-0000-000000000010"), new Guid("b0000001-0000-0000-0000-000000000005"), new Guid("c0000001-0000-0000-0000-000000000005"), 5.0, 1, 1 },
                { new Guid("f0000001-0000-0000-0000-000000000011"), new Guid("b0000002-0000-0000-0000-000000000002"), new Guid("c0000001-0000-0000-0000-000000000005"), 30.0, 1, 1 },
                { new Guid("f0000001-0000-0000-0000-000000000012"), new Guid("b0000002-0000-0000-0000-000000000002"), new Guid("c0000001-0000-0000-0000-000000000010"), 15.0, 1, 1 },
                { new Guid("f0000001-0000-0000-0000-000000000013"), new Guid("b0000001-0000-0000-0000-000000000004"), new Guid("c0000002-0000-0000-0000-000000000003"), 25.0, 1, 1 },
                { new Guid("f0000001-0000-0000-0000-000000000014"), new Guid("b0000001-0000-0000-0000-000000000004"), new Guid("c0000001-0000-0000-0000-000000000007"), 8.0, 1, 1 },
                { new Guid("f0000001-0000-0000-0000-000000000015"), new Guid("b0000002-0000-0000-0000-000000000003"), new Guid("c0000001-0000-0000-0000-000000000009"), 10.0, 1, 1 },
                { new Guid("f0000001-0000-0000-0000-000000000016"), new Guid("b0000002-0000-0000-0000-000000000003"), new Guid("c0000002-0000-0000-0000-000000000004"), 40.0, 1, 3 }
            });
        }

        private static void SeedQuests(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(table: "game_quests", columns: new[] { "id", "name", "description", "type", "required_count", "target_enemy_id", "target_item_id", "target_level", "prerequisite_quest_id", "min_level_requirement", "xp_reward", "gold_reward", "reward_item_id", "reward_item_quantity", "time_limit_minutes", "zone_id", "next_quest_id" }, values: new object[,]
            {
                { new Guid("e0000001-0000-0000-0000-000000000001"), "Goblin Menace", "Defeat 5 goblins threatening the village.", "KillCount", 5, new Guid("b0000001-0000-0000-0000-000000000001"), null, null, null, 1, 100, 50, null, 1, 0, new Guid("a0000001-0000-0000-0000-000000000001"), new Guid("e0000001-0000-0000-0000-000000000002") },
                { new Guid("e0000001-0000-0000-0000-000000000002"), "The Goblin King", "Slay the Goblin King lurking deep in the woods.", "BossDefeat", 1, new Guid("b0000002-0000-0000-0000-000000000001"), null, null, new Guid("e0000001-0000-0000-0000-000000000001"), 5, 300, 150, new Guid("c0000001-0000-0000-0000-000000000004"), 1, 0, new Guid("a0000001-0000-0000-0000-000000000001"), null },
                { new Guid("e0000001-0000-0000-0000-000000000003"), "Undead Rising", "Defeat 10 skeletons in the catacombs.", "KillCount", 10, new Guid("b0000001-0000-0000-0000-000000000002"), null, null, null, 10, 250, 120, null, 1, 0, new Guid("a0000001-0000-0000-0000-000000000002"), new Guid("e0000001-0000-0000-0000-000000000004") },
                { new Guid("e0000001-0000-0000-0000-000000000004"), "Banish the Lich", "Defeat the Lich Lord to cleanse the catacombs.", "BossDefeat", 1, new Guid("b0000002-0000-0000-0000-000000000002"), null, null, new Guid("e0000001-0000-0000-0000-000000000003"), 15, 800, 400, new Guid("c0000001-0000-0000-0000-000000000010"), 1, 0, new Guid("a0000001-0000-0000-0000-000000000002"), null },
                { new Guid("e0000001-0000-0000-0000-000000000005"), "Reach the Peak", "Reach level 30 to prove your strength.", "ReachLevel", 1, null, null, 30, null, 25, 500, 300, null, 1, 0, new Guid("a0000001-0000-0000-0000-000000000003"), new Guid("e0000001-0000-0000-0000-000000000006") },
                { new Guid("e0000001-0000-0000-0000-000000000006"), "Slay the Dragon", "Defeat the Ancient Dragon atop Dragon's Peak.", "BossDefeat", 1, new Guid("b0000002-0000-0000-0000-000000000003"), null, null, new Guid("e0000001-0000-0000-0000-000000000005"), 30, 2000, 1000, new Guid("c0000001-0000-0000-0000-000000000009"), 1, 0, new Guid("a0000001-0000-0000-0000-000000000003"), null }
            });
        }

        #endregion
    }
}
