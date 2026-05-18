using System;
using System.Collections.Concurrent;
using KodakkuAssist.Module.GameEvent;
using KodakkuAssist.Script;
using KodakkuAssist.Module.GameEvent.Struct;
using KodakkuAssist.Module.Draw;
// using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Numerics;
using Newtonsoft.Json;
using System.Linq;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
// using System.DirectoryServices;
using System.Xml.Linq;
using CicerosKodakkuAssist.FuturesRewrittenUltimate;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility.Numerics;
using KodakkuAssist.Module.GameOperate;
using Newtonsoft.Json.Linq;

namespace CicerosKodakkuAssist.FuturesRewrittenUltimate
{

    [ScriptType(name: "Karlin's FRU script (Customized by Cicero) | Karlinçš„ç»ä¼Šç”¸è„šæœ¬ (çµè§†æ”¹è£…ç‰ˆ)",
        territorys: [1238],
        guid: "1943f3fd-ab45-43b7-8dc9-828fb3085d2d",
        version: "0.0.1.19",
        note: notesOfTheScript,
        author: "Linoa235")]

    public class Futures_Rewritten_Ultimate
    {
        /*
        const string notesOfTheScript=
        """
        ***** Please read the note here carefully before running the script! *****
        ***** è¯·åœ¨ä½¿ç”¨æ­¤è„šæœ¬å‰ä»”ç»†é˜…è¯»æ­¤å¤„çš„è¯´æ˜Ž! *****

        This is a customized version of Karlin's script for Futures Rewritten (Ultimate).
        The script was branched out from the version 0.0.0.10 and extensively customized by Cicero.
        Please configure the user settings of the script according to your user settings of the vanilla script before running it!
        And of course, please don't run the customized script and the vanilla script simultaneously.
        If you would like to stream, there's no forced Vfx drawing in the script. Therefore, you could run it while streaming after proper configurations.

        è¿™æ˜¯Karlinçš„å¦ä¸€ä¸ªæœªæ¥(ç»ä¼Šç”¸)è„šæœ¬çš„æ”¹è£…ç‰ˆæœ¬ã€‚
        è„šæœ¬æ˜¯åŸºäºŽ0.0.0.10ç‰ˆæœ¬çš„,çµè§†å¯¹è„šæœ¬è¿›è¡Œäº†å¤§å¹…åº¦æ”¹è£…ã€‚
        åœ¨ä½¿ç”¨å‰è¯·è®°å¾—æŒ‰ç…§åŽŸç‰ˆè„šæœ¬é‡æ–°é…ç½®ä¸€ä¸‹è¿™ä¸ªè„šæœ¬çš„ç”¨æˆ·è®¾ç½®!
        å½“ç„¶ä¹Ÿè¯·ä¸è¦åŒæ—¶å¼€ç€æ”¹è£…è„šæœ¬å’ŒåŽŸç‰ˆè„šæœ¬ã€‚
        å¦‚æžœä½ æœ‰ç›´æ’­éœ€æ±‚,è„šæœ¬ä¸­æ²¡æœ‰å¼ºåˆ¶ä½¿ç”¨Vfxçš„ç»˜å›¾,æ‰€ä»¥ç»è¿‡é€‚å½“çš„é…ç½®å¯ä»¥ç›´æ’­æ—¶ä½¿ç”¨ã€‚

        1. The entire set of default settings is consistent with Moglin Meow's FRU strat video, except the settings which have multiple branches in Moglin Meow's Triggers.
        Regarding settings without a specified default option, its actual default settings would be based on the enumeration values in the code.
        2. Two types of TTS prompts are provided, vanilla TTS and Daily Routines TTS.
        Please make sure you only enable one of the two options. You couldn't run the both TTS simultaneously.
        And of course you need to have the plugin Daily routines installed and enabled if you'd like to use Daily Routines TTS.
        The language of TTS prompts would be consistent with that of text prompts.
        3. For any marking feature, please make sure that only one member in the party enables it, and the party are not running any similar marking feature from other plugins or triggers.
        4. There may not be exact 2 players being marked during Utopian Sky if the initial positions of players are unbearably deviant. The marks would be based on the real-time positions of each player.
        5. Marks during Fall Of Faith in Phase 1 involve three different types: Target To Attack, Target To Bind, Target To Ignore.
        Target To Ignore 1 and 2: The tethered players in the north. Numbers stand for the round of the play.
        Target To Bind 1 and 2: The tethered players in the south. Numbers stand for the round of the play.
        Target To Attack 1 and 2: The untethered players in the north. Number 1 stands for the player with higher priority.
        Target To Attack 3 and 4: The untethered players in the north. Number 3 stands for the player with higher priority.
        (Assuming the priority is MT OT H1 H2 M1 M2 R1 R2, then:
           Higher priority <- MT OT H1 H2 M1 M2 R1 R2 -> Lower priority)
        6. In the descriptions of Mirror Mirror strats in Phase 2, the left and right here refer to the left and right while facing red mirrors from the center.
        7. It's required to select a proper initial position for your Light Rampant strat, otherwise the guidance may be not reliable.
        For example, if you've selected Star_Of_David_Japanese_PF, your should also select one of the two positions where supporters are all in the north.
        8. If a player is in position and facing the Boss at the end of the first half of Phase 3, the player will definitely not be affected by Shadoweye.
        In some cases it could be hard for the players deal with the last mechanism to be in position on time. An alternative would be looking at the light belongs to the player.
        9. If Moglin_Meow_Or_Baby_Wheelchair_Based_On_Signs was selected for the second half of Phase 3, then all following drawing and guidance would be based on the signs on players from Moglin Meow's Triggers or the Baby Wheelchair Triggers.
        Attack 1 to 4 stand for the players in the left group, bind 1 to three & square stand for the players on the right. The marks with the number 1 or 2 stand for the players on melee positions.
        Missing signs or incorrect signs will cause unreliable results.
        10. Marks for players with Wyrmclaw (red) debuffs during the second half of Phase 4 involve Target To Ignore and Target To Bind.
        For the marking logic Ignore1_And_Bind1_Go_West:
        Target To Ignore 1 and Target To Bind 1: The players who are going to deal with the mechanism in the west. Target To Bind stands for the longer debuff.
        Target To Ignore 2 and Target To Bind 2: The players who are going to deal with the mechanism in the east. Target To Bind stands for the longer debuff.
        For the marking logic Ignore1_And_Ignore2_Go_West:
        Target To Ignore 1 and 2: The players who are going to deal with the mechanism in the west. Number 2 stands for the longer debuff.
        Target To Bind 1 and 2: The players who are going to deal with the mechanism in the east. Number 2 stands for the longer debuff.
        The mark priority would be based on the priority of players with Wyrmclaw (red) above.
        11. If the logic of residue guidance was set to According_To_Signs_On_Me, then the guidance would be based on the signs (Attack 1 to 4) on yourself from other plugins, triggers or manually marking.
        Meanwhile, the option According_To_Signs_On_Me could work perfectly combined with the marking feature above for the second half of Phase 4.
        If the logic of residue guidance was set to According_To_Debuffs, then the guidance would be based on the debuffs. All signs would be ignored.
        12. If a player with the Wyrmclaw (the red debuff) takes a residue from Drachen Wanderers, or a player with the Wyrmfang (the blue debuff) hits a Drachen Wanderers,
        the related drawing may be removed with delay and may cause some confusion in the second half of Phase 4.
        Anyway, those situations are pretty much already a wipe. Aside from that, fixing this issue is technically difficult, so I'll just leave it there.
        13. The guidance of Fulgent Blade in Phase 5 would be always composed of two steps, one is the current step (green by default) and the other is the next step (yellow by default).
        Please be aware that you should never move to the next step in advance, until its colour changes to the safe colour. The guidance of the next step is just a preview, which could make you be ready for it.
        14. This script could run with the patch script from @usamilyan4608, which offers further improvements and refinements in many details.
        The patch script from @usamilyan4608 could be found in his online script repository.
        15. It's highly recommended to run the script while running the plugin A Realm Record (ARR) and enabling its recording feature.
        If you encounter any issue or bug, leave the duty to cut off the recording (which would help me quickly pinpoint the pull with issues).
        After that, please describe the issue and share the related ARR recording with me. Appreciate your help!

        1. æ•´å¥—é»˜è®¤è®¾ç½®æ˜¯è·ŸèŽ«çµå–µè§†é¢‘æ”»ç•¥ä¿æŒä¸€è‡´çš„,é™¤äº†èŽ«çµå–µè§¦å‘å™¨ä¸­æä¾›å¤šä¸ªé€‰æ‹©çš„éƒ¨åˆ†ã€‚
        æ²¡æœ‰é»˜è®¤é€‰é¡¹çš„è®¾ç½®,å…¶å®žé™…é»˜è®¤è®¾ç½®å°†æ ¹æ®ä»£ç ä¸­çš„æžšä¸¾ç±»åž‹å®žé™…å€¼å†³å®šã€‚
        2. æä¾›ä¸¤ç§ç±»åž‹çš„TTSæ’­æŠ¥,åŽŸç‰ˆTTSå’ŒDaily Routines TTSã€‚
        è¯·ç¡®ä¿ä½ åªå¯ç”¨äº†äºŒè€…å…¶ä¸€,è¿™ä¸¤ä¸ªä¸èƒ½åŒæ—¶å¼€ã€‚
        å½“ç„¶,å¦‚æžœé€‰æ‹©äº†Daily Routines TTS,ä½ éœ€è¦å·²ç»å®‰è£…å¹¶å¯ç”¨äº†Daily Routinesæ’ä»¶ã€‚
        TTSæç¤ºçš„è¯­è¨€ä¸Žæ–‡æœ¬æç¤ºè¯­è¨€ç›¸åŒã€‚
        3. å¯¹äºŽä»»ä½•æ ‡è®°é˜Ÿå‹çš„åŠŸèƒ½,å°é˜Ÿé‡Œåªèƒ½æœ‰ä¸€ä¸ªçŽ©å®¶å¯ç”¨,å¹¶ä¸”ä¹Ÿä¸ä¸Žå…¶ä»–ç§‘æŠ€çš„å…¼å®¹ã€‚
        4. P1ä¹å›­ç»æŠ€(é›¾é¾™)æœŸé—´è¢«æ ‡è®°çš„çŽ©å®¶å¯èƒ½å°‘äºŽæˆ–è€…å¤šäºŽ2äºº,å› ä¸ºä¼šæ•èŽ·çŽ©å®¶çš„å®žæ—¶ä½ç½®è¿›è¡Œè®¡ç®—,ç«™ä½ä¸æ ‡å‡†å¯èƒ½ä¼šå¯¼è‡´è¿™ç§æƒ…å†µã€‚
        5. P1ä¿¡ä»°å´©å¡Œ(å››è¿žæŠ“)æœŸé—´çš„æ ‡è®°æ¶‰åŠåˆ°æ”»å‡»,æ­¢æ­¥å’Œç¦æ­¢ä¸‰ç§ã€‚
        ç¦æ­¢1å’Œ2:å‰å¾€åŒ—ä¾§çš„è¢«è¿žçº¿çŽ©å®¶ã€‚æ•°å­—å°±æ˜¯æŠ“äººçš„è½®æ•°ã€‚
        é”é“¾1å’Œ2:å‰å¾€å—ä¾§çš„è¢«è¿žçº¿çŽ©å®¶ã€‚æ•°å­—å°±æ˜¯æŠ“äººçš„è½®æ•°ã€‚
        æ”»å‡»1å’Œ2:å‰å¾€åŒ—ä¾§çš„é—²äººã€‚æ•°å­—1æ˜¯ä¼˜å…ˆçº§æ›´é«˜çš„ã€‚
        æ”»å‡»3å’Œ4:å‰å¾€å—ä¾§çš„é—²äººã€‚æ•°å­—3æ˜¯ä¼˜å…ˆçº§æ›´é«˜çš„ã€‚
        (å‡è®¾ä¼˜å…ˆçº§ä¸º MT ST H1 H2 D1 D2 D3 D4, é‚£ä¹ˆé«˜ä¼˜å…ˆçº§æŒ‡çš„æ˜¯:
           é«˜ä¼˜å…ˆçº§ <- MT ST H1 H2 D1 D2 D3 D4 -> ä½Žä¼˜å…ˆçº§)
        6. P2é•œä¸­å¥‡é‡æ”»ç•¥çš„æè¿°ä¸­,å·¦å’Œå³çš„åŸºå‡†æŒ‡çš„æ˜¯ä»Žåœºä¸­é¢å‘ä¸¤é¢çº¢è‰²é•œå­æ—¶çš„å·¦å’Œå³ã€‚
        7. P2å…‰ä¹‹å¤±æŽ§(å…‰æš´)å¿…é¡»åœ¨é…ç½®ä¸­è®¾å®šç›¸åº”çš„é¢„ç«™ä½,å¦åˆ™æŒ‡è·¯ä¼šç”µæ¤…ã€‚
        ä¾‹å¦‚å¦‚æžœä½ é€‰æ‹©"å…­èŠ’æ˜Ÿæ—¥æœé‡Žé˜Ÿæ³•",ä½ å¿…é¡»è¦é€‰æ‹©ä¸¤ç§è“ç»¿åœ¨åŒ—åŠåœºçš„é¢„ç«™ä½ä¹‹ä¸€ã€‚
        8. P3ä¸€è¿çš„æœ€åŽå¦‚æžœçŽ©å®¶ç«™åœ¨æŒ‡è·¯çš„ç‚¹ä¸Šå¹¶é¢å‘Boss,å°±ä¸€å®šä¸ä¼šåƒåˆ°æš—å½±ä¹‹çœ¼(çŸ³åŒ–çœ¼)ã€‚
        å€¼å¾—ä¸€æçš„æ˜¯å¤„ç†æœ€åŽæœºåˆ¶çš„äººå¯èƒ½æ¥ä¸åŠå°±ä½,è¿™ç§æƒ…å†µä¸‹å¯ä»¥é€šè¿‡é¢å‘è‡ªå·±çš„ç¯è§£å†³ã€‚
        9. å¦‚æžœP3äºŒè¿é€‰æ‹©äº†"æ ¹æ®ç›®æ ‡æ ‡è®°çš„èŽ«çµå–µæ³•æˆ–å®å®æ¤…æ³•",åˆ™æŽ¥ä¸‹æ¥å‡ ä¹Žæ‰€æœ‰ç»˜åˆ¶å’ŒæŒ‡è·¯éƒ½ä¼šä¾èµ–æ¥è‡ªèŽ«çµå–µè§¦å‘å™¨æˆ–è€…å®å®æ¤…çš„ç›®æ ‡æ ‡è®°ã€‚
        æ”»å‡»1åˆ°4ä»£è¡¨åŽ»å·¦ç»„çš„äºº,æ­¢æ­¥1åˆ°3å’Œæ–¹å—ä»£è¡¨åŽ»å³ç»„çš„ã€‚ä¸‹æ ‡1å’Œ2è¡¨ç¤ºç«™è¿‘æˆ˜ä½ã€‚
        æ®‹ç¼ºæˆ–è€…é”™è¯¯çš„æ ‡è®°å°†å¯¼è‡´ç”µæ¤…ã€‚
        10. P4äºŒè¿å¯¹åœ£é¾™çˆª(çº¢)debuffçŽ©å®¶çš„æ ‡è®°æ¶‰åŠåˆ°ç¦æ­¢å’Œæ­¢æ­¥ã€‚
        å¯¹äºŽç¦æ­¢1å’Œé”é“¾1åŽ»è¥¿è¾¹çš„æ ‡è®°é€»è¾‘æ¥è¯´:
        ç¦æ­¢1å’Œé”é“¾1:å‰å¾€è¥¿ä¾§å¤„ç†æœºåˆ¶ã€‚é”é“¾æ˜¯é•¿debuffã€‚
        ç¦æ­¢2å’Œé”é“¾2:å‰å¾€ä¸œä¾§å¤„ç†æœºåˆ¶ã€‚é”é“¾æ˜¯é•¿debuffã€‚
        å¯¹äºŽç¦æ­¢1å’Œç¦æ­¢2åŽ»è¥¿è¾¹çš„æ ‡è®°é€»è¾‘æ¥è¯´:
        ç¦æ­¢1å’Œ2:å‰å¾€è¥¿ä¾§å¤„ç†æœºåˆ¶ã€‚æ•°å­—2æ˜¯é•¿debuffã€‚
        æ­¢æ­¥1å’Œ2:å‰å¾€ä¸œä¾§å¤„ç†æœºåˆ¶ã€‚æ•°å­—2æ˜¯é•¿debuffã€‚
        æ ‡è®°çš„ä¼˜å…ˆçº§å°†å–å†³äºŽä¸Šé¢åœ£é¾™çˆª(çº¢)çŽ©å®¶ä¼˜å…ˆçº§çš„è®¾ç½®ã€‚
        11. å¦‚æžœP4äºŒè¿çš„ç™½åœˆæŒ‡è·¯é€»è¾‘è®¾ç½®ä¸º"æ ¹æ®æˆ‘èº«ä¸Šçš„ç›®æ ‡æ ‡è®°",åˆ™ä¼šæ ¹æ®æ¥è‡ªå…¶ä»–ç§‘æŠ€æˆ–è€…æ‰‹æ‘‡çš„è‡ªèº«çš„æ ‡è®°(æ”»å‡»1åˆ°4)æŒ‡è·¯ã€‚
        åŒæ—¶,é€‰é¡¹"æ ¹æ®æˆ‘èº«ä¸Šçš„ç›®æ ‡æ ‡è®°"å¯ä»¥æœ‰æ•ˆé…åˆä¸Šé¢çš„P4äºŒè¿æ ‡è®°åŠŸèƒ½ä½¿ç”¨ã€‚
        å¦‚æžœP4äºŒè¿çš„ç™½åœˆæŒ‡è·¯é€»è¾‘è®¾ç½®ä¸º"æ ¹æ®Debuff",åˆ™åªä¼šæ ¹æ®DebuffæŒ‡è·¯,æ ‡è®°å°†å®Œå…¨è¢«æ— è§†ã€‚
        12. å¦‚æžœP4äºŒè¿æŒæœ‰åœ£é¾™çˆª(çº¢)debuffçš„çŽ©å®¶åƒäº†åœ£é¾™æ°”æ¯(é¾™å¤´)çš„ç™½åœˆ,æˆ–è€…æŒæœ‰åœ£é¾™ç‰™(è“)debuffçš„çŽ©å®¶æ’žäº†åœ£é¾™æ°”æ¯(é¾™å¤´),
        é‚£ä¹ˆç›¸å…³ç»˜åˆ¶çš„ç§»é™¤å¯èƒ½æœ‰å»¶è¿Ÿå¹¶ä¸”ä¼šå¹²æ‰°çŽ©å®¶ã€‚
        ä¸è¿‡å¦‚æžœå·²ç»è¿™æ ·é‚£å¤§æ¦‚çŽ‡æ˜¯è¦å›¢ç­äº†,ä¿®å¤è¿™ä¸ªé—®é¢˜åœ¨æŠ€æœ¯å±‚é¢ä¸Šä¹Ÿæœ‰ç‚¹éš¾åº¦,æ‰€ä»¥æˆ‘å°±ä¸ç®¡äº†ã€‚
        13. P5ç’€ç’¨ä¹‹åˆƒ(åœ°ç«)æŒ‡è·¯è¢«åˆ†ä¸ºä¸¤ä¸ªéƒ¨åˆ†,ä¸€éƒ¨åˆ†æ˜¯å½“å‰æ­¥(é»˜è®¤ç»¿è‰²),å¦ä¸€éƒ¨åˆ†æ˜¯ä¸‹ä¸€æ­¥(é»˜è®¤é»„è‰²)ã€‚
        ç›´åˆ°ä¸‹ä¸€æ­¥å˜æˆå®‰å…¨è‰²ä¹‹å‰,æ°¸è¿œä¸è¦æå‰ç§»åŠ¨ã€‚ä¸‹ä¸€æ­¥çš„ç»˜åˆ¶ä»…ä½œé¢„è§ˆç”¨é€”,è®©ä½ æœ‰ä¸ªå¿ƒç†å‡†å¤‡ã€‚
        14. æ­¤è„šæœ¬å¯ä»¥ä¸Ž@usamilyan4608çš„ç»ä¼Šç”¸è¡¥ä¸è„šæœ¬ä¸€èµ·ä½¿ç”¨,è¿™ä¸ªè¡¥ä¸è„šæœ¬è¿›ä¸€æ­¥åœ¨è®¸å¤šç»†èŠ‚ä¸Šæä¾›äº†ä¼˜åŒ–å’Œç²¾ä¿®ã€‚
        è¡¥ä¸è„šæœ¬å¯ä»¥åœ¨@usamilyan4608çš„ä¸ªäººåœ¨çº¿è„šæœ¬åº“ä¸­æ‰¾åˆ°ã€‚
        15. éžå¸¸å»ºè®®åœ¨ç”¨è¿™ä¸ªè„šæœ¬æ‰“æœ¬çš„åŒæ—¶,å¯ç”¨æ’ä»¶A Realm Record(ARR)å¹¶å¼€å¯å½•åˆ¶ã€‚
        å¦‚æžœé‡åˆ°äº†é—®é¢˜æˆ–bug,è¯·é€€æœ¬ä¸€æ¬¡æ¥åˆ‡æ–­å½•åƒ(è¿™æ ·æˆ‘èƒ½å¿«é€Ÿå®šä½å‡ºé—®é¢˜çš„é‚£ä¸€æŠŠ)ã€‚
        ç„¶åŽ,ç®€å•æè¿°ä¸€ä¸‹é—®é¢˜å¹¶åˆ†äº«ä¸€ä¸‹é‚£ä»½å‡ºäº†é—®é¢˜çš„ARRå½•åƒã€‚éžå¸¸æ„Ÿè°¢!

        ***** Credits *****
        ***** è‡´è°¢ *****

        The original author, the founder and the co-maintainer: @karlin_z
        Helpers (sorted lexicographically):
         - @abigseal provided Fixed_H1_H2_R2_The_Rest_Fill_Vacancies for towers at the end of Phase 1. (Mar 9, 2025)
         - @alexandria_prime provided Single_Line_In_HTD_Order, Single_Line_In_H1TDH2_Order and Face_The_Boss for Fall Of Faith in Phase 1. (Mar 5, 2025)
         - @bupleurum. provided affixes regarding the MMW strats on CN, optimized paths of the New Grey9 strat for Light Rampant in Phase 2. (Mar 20, 2025)
         - @cyf5119 provided ranges of halo AOEs for Turn of the Heavens in Phase 1. (Mar 19, 2025)
         - @milkvio provided guidance for Fulgent Blade in Phase 5. (Mar 16, 2025)
         - @usamilyan4608 provided warnings by time for AOEs from spheres during Light Rampant in Phase 2, optimized drawing of Drachen Wanderers in the second half of Phase 4, incorrect debug output in the developer mode.
           @usamilyan4608 also provided a valuable recording involves a rare local exception during the second half of Phase 3 with some fix suggestions.
           (Mar 16, 2025; Mar 22, 2025; Mar 24, 2025; Apr 7, 2025)
         - @veever2464 provided supports of Daily Routines TTS for each TTS prompt. (Mar 10, 2025)

        åŽŸä½œè€…,å¥ åŸºäººå…¼å…±åŒç»´æŠ¤è€…: @karlin_z
        æä¾›å¸®åŠ©çš„äºº(æŒ‰å­—å…¸åºæŽ’åº):
        - @abigsealä¸ºP1æœ«å°¾è¸©å¡”æä¾›äº†æ‰“æ³•"å›ºå®šH1_H2_D4å‰©ä½™äººè¡¥ä½"ã€‚ (2025.03.09)
        - @alexandria_primeä¸ºP1ä¿¡ä»°å´©å¡Œ(å››è¿žæŠ“)æä¾›äº†æ‰“æ³•"æŒ‰HTDé¡ºåºå•æŽ’","æŒ‰H1TDH2é¡ºåºå•æŽ’"å’Œ"é¢å‘Boss"ã€‚ (2025.03.05)
        - @bupleurum.ä¸ºé…ç½®æ–‡æœ¬æä¾›äº†å›½æœMMWæ”»ç•¥ç›¸å…³æ ‡æ³¨,P2å…‰ä¹‹å¤±æŽ§(å…‰æš´)æ–°ç°ä¹æ³•å…‰æµä¾µèš€(æ”¾æ³¥)å¤§è‡´è·¯å¾„ç»†åŒ–ã€‚ (2025.3.20)
        - @cyf5119ä¸ºP1å…‰è½®å¬å”¤æä¾›äº†é›·ç„°ä¹‹å…‰è½®çš„AOEèŒƒå›´ã€‚ (2025.3.19)
        - @milkvioä¸ºP5ç’€ç’¨ä¹‹åˆƒ(åœ°ç«)æä¾›äº†æŒ‡è·¯ã€‚ (2025.03.16)
        - @usamilyan4608ä¸ºP2å…‰ä¹‹å¤±æŽ§(å…‰æš´)æœŸé—´çš„å…‰çƒAOEæä¾›äº†æ—¶é—´è­¦å‘Š,P4äºŒè¿é¾™å¤´ç»˜åˆ¶ä¼˜åŒ–,å¼€å‘è€…æ¨¡å¼è°ƒè¯•è¾“å‡ºä¿®å¤ã€‚
          @usamilyan4608è¿˜ä¸ºP3äºŒè¿çš„ç½•è§æœ¬åœ°é”™è¯¯æä¾›äº†çè´µå½•åƒå’Œä¿®å¤å»ºè®®ã€‚
          (2025.03.16, 2025.03.22, 2025.03.24, 2025.04.07)
        - @veever2464ä¸ºæ¯ä¸€æ¡TTSæç¤ºæä¾›äº†Daily Routines TTSæ”¯æŒã€‚ (2025.03.10)

        ***** New Features *****
        ***** æ–°åŠŸèƒ½ *****

        Phase 1:
         - Refinements for the entire phase;
         - New strats for Utopian Sky, Fall Of Faith and the towers at the end;
         - Player marking for Utopian Sky;
         - Reworked player marking during Fall Of Faith;
        Phase 2:
         - Reworked guidance after the knockback during Diamond Dust.
         - Reworked guidance of Mirror, Mirror.
         - Fixes and refinements for Light Rampant;
        Phase 3:
         - Guidance of the second half (including the Double Group strat, the Locomotive strat and the Moglin Meow strat or Baby Wheelchair strat based on signs);
        Phase 4:
         - New strat of the first half (Single Swap).
         - Guidance related to Drachen Wanderer residues of the second half;
         - Fixes and refinements for vanilla guidance of the second half;
         - New strat (HTD priority) and player marking of the second half;
        Phase 5:
         - Guidance of Fulgent Blade;
         - Guidance of Wings Dark And Light (including the Grey9 Brain Dead strat and the Reverse Triangle strat);
         - Guidance of Polarizing Strikes.

        P1:
         - æ•´ä¸ªé˜¶æ®µçš„ç²¾ä¿®;
         - ä¹å›­ç»æŠ€(é›¾é¾™),ä¿¡ä»°å´©å¡Œ(å››è¿žæŠ“)å’Œæœ€åŽè¸©å¡”éƒ½å¢žåŠ äº†æ–°æ”»ç•¥;
         - ä¹å›­ç»æŠ€(é›¾é¾™)çš„çŽ©å®¶æ ‡è®°;
         - é‡åšä¿¡ä»°å´©å¡Œ(å››è¿žæŠ“)çš„çŽ©å®¶æ ‡è®°;
        P2:
         - é’»çŸ³æ˜Ÿè¾°å‡»é€€åŽæŒ‡è·¯é‡åš;
         - é•œä¸­å¥‡é‡æŒ‡è·¯é‡åš;
         - å…‰ä¹‹å¤±æŽ§(å…‰æš´)ä¿®å¤å’Œç»†åŒ–;
        P3:
         - äºŒè¿æŒ‡è·¯(åŒ…æ‹¬åŒåˆ†ç»„æ³•,è½¦å¤´æ³•å’ŒåŸºäºŽæ ‡è®°çš„èŽ«çµå–µæˆ–å®å®æ¤…æ³•);
        P4:
         - ä¸€è¿æ–°æ”»ç•¥(å•æ¢);
         - äºŒè¿åœ£é¾™æ°”æ¯(é¾™å¤´)ç™½åœˆç›¸å…³çš„æŒ‡è·¯;
         - äºŒè¿åŽŸç‰ˆæŒ‡è·¯ä¿®å¤å’Œç»†åŒ–;
         - äºŒè¿æ–°æ”»ç•¥(HTDä¼˜å…ˆçº§)ä¸ŽçŽ©å®¶æ ‡è®°;
        P5:
         - ç’€ç’¨ä¹‹åˆƒ(åœ°ç«)æŒ‡è·¯;
         - å…‰ä¸Žæš—ä¹‹ç¿¼(è¸©å¡”)æŒ‡è·¯(åŒ…æ‹¬ç°ä¹è„‘æ­»æ³•å’Œå€’ä¸‰è§’æ³•);
         - æžåŒ–æ‰“å‡»(æŒ¡æžª)æŒ‡è·¯ã€‚

        ***** Known Issues *****
        ***** å·²çŸ¥é—®é¢˜ *****

        Phase 3:
         - Ultimate Relativity: The guidance of Sinbound Meltdown may disappear a tiny bit earlier than the time that the direction is anchored. It's not fatal by any mean, but it's always recommended that baiting it precisely before leaving.
           The timeline here would be refined in the future.

        After all the known issues are resolved, there will be no more major update. The version will be considered as the final version.

        P3:
         - æ—¶é—´åŽ‹ç¼©Â·ç»(ä¸€è¿): ç½ªç¼šç†”æ¯(æ¿€å…‰)çš„æŒ‡è·¯å˜åŒ–æ—¶é—´å¯èƒ½ç•¥å¾®æ—©äºŽå®žé™…åˆ¤å®šæ—¶é—´ä¸€ç‚¹ç‚¹ã€‚ä¸æ˜¯å¤§é—®é¢˜,ä½†æœ€å¥½è¿˜æ˜¯ç¡®ä¿å¼•å¯¼åˆ°äº†ä»¥åŽå†ç§»åŠ¨ã€‚
           ä¼šåœ¨æœªæ¥ç²¾ä¿®æ­¤å¤„çš„æ—¶é—´è½´ã€‚

        å½“æ‰€æœ‰å·²çŸ¥é—®é¢˜éƒ½è¢«è§£å†³åŽ,å°±ä¸ä¼šå†æœ‰å¤§æ›´æ–°äº†ã€‚é‚£ä¸ªæ—¶å€™çš„ç‰ˆæœ¬å°±æ˜¯æœ€ç»ˆç‰ˆã€‚

        ***** To Resellers *****
        ***** è‡´å€’å–è€… *****

        I sincerely wish:
        Your life will be like Nero;
        Your luck will be like Diocletian;
        Your reputation will be like Commodus;
        Your integrity will be like Caracalla;
        Your morality will be like Elagabalus;
        Your credibility will be like Gallienus;
        Your future will be like Severus Alexander;
        And your end will be like Romulus Augustulus.
        
        æˆ‘è¡·å¿ƒåœ°ç¥æ„¿:
        ä½ çš„äººç”Ÿåƒå°¼ç¦„;
        ä½ çš„è¿æ°”åƒæˆ´å…‹é‡Œå…ˆ;
        ä½ çš„åèª‰åƒåº·èŒ‚å¾·;
        ä½ çš„è¯šä¿¡åƒå¡æ‹‰å¡æ‹‰;
        ä½ çš„é“å¾·åƒåŸƒæ‹‰ä¼½å·´è·¯æ–¯;
        ä½ çš„ä¿¡ç”¨åƒåŠ é‡Œæ©åŠªæ–¯;
        ä½ çš„æœªæ¥åƒå¡žç»´é²Â·äºšåŽ†å±±å¤§;
        è€Œä½ çš„ç»“å±€åƒç½—æ…•è·¯æ–¯Â·å¥¥å¤æ–¯éƒ½ã€‚

        """;
        */

        const string notesOfTheScript =
        """
        ***** Please read the note here carefully before running the script! *****
        
        There is a character limit in the description area of scripts that I'm unable to put the whole description here. Therefore, I moved the script description to the pinned messages on Discord.
        Navigate to the Discord of Kodakku Assist, find the post "Cicero's Kodakku Assist ä¸ªäººåœ¨çº¿è„šæœ¬åº“" in the channel "ç¤ºä¾‹ä¸Žåˆ†äº«", and finally check "Pinned Messages" for the script description.
        For PC, "Pinned Messages" is in the upper right corner of the chat bar. For mobile, click the arrow icon in the upper right corner, then there would be a tab "Pins".
        It would take about 5 minutes to go through the English part of the description. Please make sure you have read it in full before running the script. Thank you!
        
        ***** è„šæœ¬æè¿°éƒ¨åˆ†æœ‰å­—æ•°é™åˆ¶,æ²¡æ³•æ”¾ä¸‹æ•´ä¸ªæè¿°,æ‰€ä»¥æˆ‘æŠŠæè¿°éƒ¨åˆ†ç§»åˆ°äº†Discordä¸Šçš„æ ‡æ³¨æ¶ˆæ¯ä¸­ã€‚ *****
        åŽ»å¯è¾¾é¸­çš„Discord,åœ¨"ç¤ºä¾‹ä¸Žåˆ†äº«"é¢‘é“ä¸­æ‰¾åˆ°å¸–å­"Cicero's Kodakku Assist ä¸ªäººåœ¨çº¿è„šæœ¬åº“",é€‰æ‹©"å·²æ ‡æ³¨æ¶ˆæ¯",å°±å¯ä»¥æŸ¥çœ‹è„šæœ¬æè¿°äº†ã€‚
        å¯¹äºŽç”µè„‘ç«¯,"å·²æ ‡æ³¨æ¶ˆæ¯"åœ¨èŠå¤©æ çš„å³ä¸Šè§’ã€‚å¯¹äºŽæ‰‹æœºç«¯,ç‚¹å‡»å³ä¸Šè§’çš„ç®­å¤´æ ‡å¿—,ç„¶åŽå¯ä»¥æ‰¾åˆ°ä¸€ä¸ªåä¸º"æ ‡æ³¨"çš„æ ‡ç­¾é¡µã€‚
        é˜…è¯»å®Œä¸­æ–‡éƒ¨åˆ†å¤§çº¦éœ€è¦èŠ±è´¹5åˆ†é’Ÿçš„æ—¶é—´ã€‚è¯·å…ˆå®Œæ•´åœ°é˜…è¯»è„šæœ¬æè¿°å†ä½¿ç”¨æœ¬è„šæœ¬ï¼Œè°¢è°¢ï¼
        
        
        
        Scipio Aemilianus (Scipio the Younger) gazed long upon the vast city of Carthage below. For 700 long years, Carthage had possessed broad lands, numerous islands, and a dominion over the seas.
        The vast armaments, warships, elephants, and immense wealth of Carthage were in no way inferior to any mighty empire before it in human history.
        Yet today, the city had fallen, utterly destroyed, wiped from the face of the earth. Moved by the fate of his enemy, Scipio Aemilianus shed tears. His heart stirred, his thoughts transcending the usual mindset of a military conqueror.
        He realized that not only humans, but cities, states, and even the mightiest empires could not escape the fate of ruin. Troy, Assyria, Persia, and the Kingdom of Macedon twenty years earlier had all followed the iron law shown by history: "The victor must inevitably fall."
        Whether by design or chance, the supreme commander of the victorious army recited a famous line from Homer's Iliad, spoken by the Trojan hero Hector:
        "Troy, I suppose, will also perish with King Priam and the warriors who follow him!"
        The historian Polybius, standing behind him, asked why he said this. Scipio Aemilianus turned, looked at his close friend of twenty years, took his hand, and replied:
        "Polybius, we have just destroyed an empire that was once powerful, and we have won this 'great moment'. But my heart is not filled with the joy of victory. Instead, I am somewhat sad â€” I fear that our own Rome may one day suffer the same fate!"
        â€” At the fall of Carthage, near the end of the Third Punic War, 146 BC
        """;
        
        #region User_Settings

        [UserSetting("-----Global Settings----- (No actual meaning for this setting)")]
        public bool _____Global_Settings_____ { get; set; } = true;
        [UserSetting("Enable Text Prompts")]
        public bool Enable_Text_Prompts { get; set; } = true;
        [UserSetting("Language Of Prompts")]
        public Languages_Of_Prompts Language_Of_Prompts { get; set; }
        [UserSetting("Weird Shenanigans (Some random weird prompts after each wipe.)")]
        public Weird_Shenanigans Weird_Shenanigan { get; set; } = Weird_Shenanigans.Astesia_ACR_èŸ¹èŸ¹çš„ACR;

        [UserSetting("-----TTS Settings----- (No actual meaning for this setting)")]
        public bool _____TTS_Settings_____ { get; set; } = true;
        [UserSetting("Enable Vanilla TTS")]
        public bool Enable_Vanilla_TTS { get; set; } = true;
        [UserSetting("Enable Daily Routines TTS (It requires the plugin Daily Routines to be enabled already!)")]
        public bool Enable_Daily_Routines_TTS { get; set; } = false;

        [UserSetting("-----Phase 1 Settings----- (No actual meaning for this setting)")]
        public bool _____Phase1_Settings_____ { get; set; } = true;
        [UserSetting("P1 Utopian Sky Standby Position")]
        public Phase1_Standby_Positions_Of_Utopian_Sky Phase1_Standby_Position_Of_Utopian_Sky { get; set; } = Phase1_Standby_Positions_Of_Utopian_Sky.Swap_OT_And_M2_äº¤æ¢STä¸ŽD4_èŽ«çµå–µä¸ŽMMW;
        [UserSetting("P1 Mark Players In Safe Positions (Make sure only one in the party enables this!)")]
        public bool Phase1_Mark_Players_In_Safe_Positions { get; set; } = false;
        [UserSetting("P1 Colour Of Burnt Strike Characteristics")]
        public ScriptColor Phase1_Colour_Of_Burnt_Strike_Characteristics { get; set; } = new() { V4 = new(1f, 1f, 0f, 1f) };
        [UserSetting("P1 Turn Of The Heavens Groups")]
        public Phase1_Groups_Of_Turn_Of_The_Heavens Phase1_Group_Of_Turn_Of_The_Heavens { get; set; } = Phase1_Groups_Of_Turn_Of_The_Heavens.MTOTH1H2_Go_North_MTM1_vary_MTSTH1H2åŽ»åŒ—MTD1æ¢_èŽ«çµå–µä¸ŽMMW;
        [UserSetting("P1 Fall Of Faith Strat")]
        public Phase1_Strats_Of_Fall_Of_Faith Phase1_Strat_Of_Fall_Of_Faith { get; set; } = Phase1_Strats_Of_Fall_Of_Faith.Single_Line_In_HTD_Order_æŒ‰HTDé¡ºåºå•æŽ’_èŽ«çµå–µ;
        [UserSetting("P1 Mark Players During Fall Of Faith (Make sure only one in the party enables this!)")]
        public bool Phase1_Mark_Players_During_Fall_Of_Faith { get; set; } = false;
        [UserSetting("P1 Orientation Benchmarks During Fall Of Faith")]
        public Phase1_Orientation_Benchmarks_During_Fall_Of_Faith Phase1_Orientation_Benchmark_During_Fall_Of_Faith { get; set; } = Phase1_Orientation_Benchmarks_During_Fall_Of_Faith.High_Priority_Left_Facing_The_Boss_é¢å‘Bosså·¦ä¾§é«˜ä¼˜å…ˆçº§_èŽ«çµå–µä¸ŽMMW;
        [UserSetting("P1 Towers Strat")]
        public Phase1_Strats_Of_Towers Phase1_Strat_Of_Towers { get; set; } = Phase1_Strats_Of_Towers.Completely_Based_On_Priority_å®Œå…¨æ ¹æ®ä¼˜å…ˆçº§_èŽ«çµå–µ;

        [UserSetting("-----Phase 2 Settings----- (No actual meaning for this setting)")]
        public bool _____Phase2_Settings_____ { get; set; } = true;
        [UserSetting("P2 Strat After Knockback")]
        public Phase2_Strats_After_Knockback Phase2_Strat_After_Knockback { get; set; } = Phase2_Strats_After_Knockback.Clockwise_Both_Groups_Counterclockwise_æ€»æ˜¯é¡ºæ—¶é’ˆåŒç»„é€†æ—¶é’ˆ_èŽ«çµå–µä¸ŽMMW;
        [UserSetting("P2 Mirror Mirror Strat")]
        public Phase2_Strats_Of_Mirror_Mirror Phase2_Strat_Of_Mirror_Mirror { get; set; } = Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Closest_Red_Right_If_Same_è¿‘æˆ˜ç»„æœ€è¿‘çº¢è‰²é•œå­è·ç¦»ç›¸åŒåˆ™å³_èŽ«çµå–µä¸ŽMMW;
        [UserSetting("P2 Colour Of Mirror Rough Guidance")]
        public ScriptColor Phase2_Colour_Of_Mirror_Rough_Guidance { get; set; } = new() { V4 = new(1f, 1f, 0f, 1f) };
        [UserSetting("P2 Colour Of Potential Dangerous Zones")]
        public ScriptColor Phase2_Colour_Of_Potential_Dangerous_Zones { get; set; } = new() { V4 = new(1f, 0f, 0f, 1f) };
        [UserSetting("P2 Light Rampant Initial Protean Positions")]
        public Phase2_Initial_Protean_Positions_Of_Light_Rampant Phase2_Initial_Protean_Position_Of_Light_Rampant { get; set; } = Phase2_Initial_Protean_Positions_Of_Light_Rampant.Normal_Protean_Tanks_North_East_For_Both_Grey9_å¸¸è§„å…«æ–¹Tåœ¨ä¸œåŒ—_ç°9ç”¨;
        [UserSetting("P2 Light Rampant Strat")]
        public Phase2_Strats_Of_Light_Rampant Phase2_Strat_Of_Light_Rampant { get; set; } = Phase2_Strats_Of_Light_Rampant.New_Grey9_æ–°ç°ä¹æ³•_èŽ«çµå–µä¸ŽMMW;
        [UserSetting("P2 Colour Of Rough Paths")]
        public ScriptColor Phase2_Colour_Of_Rough_Paths { get; set; } = new() { V4 = new(1f, 1f, 0f, 1f) };
        [UserSetting("P2 Colour Of Sphere AOEs")]
        public ScriptColor Phase2_Colour_Of_Sphere_AOEs { get; set; } = new() { V4 = new(1f, 0f, 0f, 1f) };

        [UserSetting("-----Phase 3 Settings----- (No actual meaning for this setting)")]
        public bool _____Phase3_Settings_____ { get; set; } = true;
        [UserSetting("P3 First Half Strat")]
        public Phase3_Strats_Of_The_First_Half Phase3_Strat_Of_The_First_Half { get; set; } = Phase3_Strats_Of_The_First_Half.Moogle_èŽ«å¤åŠ›_èŽ«çµå–µä¸ŽMMW;
        [UserSetting("P3 Second Half Strat")]
        public Phase3_Strats_Of_The_Second_Half Phase3_Strat_Of_The_Second_Half { get; set; } = Phase3_Strats_Of_The_Second_Half.Moglin_Meow_Or_Baby_Wheelchair_Based_On_Signs_æ ¹æ®ç›®æ ‡æ ‡è®°çš„èŽ«çµå–µæ³•æˆ–å®å®æ¤…æ³•;
        [UserSetting("P3 Double Group Strat Branch")]
        public Phase3_Branches_Of_The_Double_Group_Strat Phase3_Branch_Of_The_Double_Group_Strat { get; set; } = Phase3_Branches_Of_The_Double_Group_Strat.Based_On_Safe_Positions_å®‰å…¨åŒºä¸ºåŸºå‡†_MMW;
        [UserSetting("P3 Locomotive Strat Branch")]
        public Phase3_Branches_Of_The_Locomotive_Strat Phase3_Branch_Of_The_Locomotive_Strat { get; set; } = Phase3_Branches_Of_The_Locomotive_Strat.Others_As_Locomotives_Chinese_PF_å›½æœé‡Žé˜Ÿäººç¾¤ä¸ºè½¦å¤´;
        [UserSetting("P3 Zone Division")]
        public Phase3_Divisions_Of_The_Zone Phase3_Division_Of_The_Zone { get; set; } = Phase3_Divisions_Of_The_Zone.North_To_Southwest_For_The_Left_Group_å·¦ç»„ä»Žæ­£åŒ—åˆ°è¥¿å—_èŽ«çµå–µä¸ŽMMW;
        [UserSetting("P3 Colour Of Rough Guidance")]
        public ScriptColor Phase3_Colour_Of_Rough_Guidance { get; set; } = new() { V4 = new(1f, 1f, 0f, 1f) };
        [UserSetting("P3 Colour Of The Penultimate Apocalypse")]
        public ScriptColor Phase3_Colour_Of_The_Penultimate_Apocalypse { get; set; } = new() { V4 = new(0, 1f, 1f, 1f) };
        [UserSetting("P3 Tank Who Baits Darkest Dance")]
        public Tanks Phase3_Tank_Who_Baits_Darkest_Dance { get; set; }
        [UserSetting("P3 Colour Of Darkest Dance")]
        public ScriptColor Phase3_Colour_Of_Darkest_Dance { get; set; } = new() { V4 = new(1f, 0f, 0f, 1f) };

        [UserSetting("-----Phase 4 Settings----- (No actual meaning for this setting)")]
        public bool _____Phase4_Settings_____ { get; set; } = true;
        [UserSetting("P4 First Half Strat")]
        public Phase4_Strats_Of_The_First_Half Phase4_Strat_Of_The_First_Half { get; set; } = Phase4_Strats_Of_The_First_Half.Single_Swap_Baiting_After_å…ˆå•æ¢å†å¼•å¯¼_èŽ«çµå–µä¸ŽMMW;
        [UserSetting("P4 Colour Of Somber Dance")]
        public ScriptColor Phase4_Colour_Of_Somber_Dance { get; set; } = new() { V4 = new(1f, 0f, 0f, 1f) };
        [UserSetting("P4 Mark Players During The Second Half (Make sure only one in the party enables this!)")]
        public bool Phase4_Mark_Players_During_The_Second_Half { get; set; } = false;
        [UserSetting("P4 Player Types To Be Marked")]
        public Phase4_Player_Types_To_Be_Marked Phase4_Player_Type_To_Be_Marked { get; set; }
        [UserSetting("P4 Priority Of The Players With Wyrmclaw")]
        public Phase4_Priorities_Of_The_Players_With_Wyrmclaw Phase4_Priority_Of_The_Players_With_Wyrmclaw { get; set; } = Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_THD_Order_æŒ‰THDé¡ºåº_èŽ«çµå–µ;
        [UserSetting("P4 Logic Of Marking Teammates With Wyrmclaw")]
        public Phase4_Logics_Of_Marking_Teammates_With_Wyrmclaw Phase4_Logic_Of_Marking_Teammates_With_Wyrmclaw { get; set; } = Phase4_Logics_Of_Marking_Teammates_With_Wyrmclaw.Ignore1_And_Bind1_Go_West_ç¦æ­¢1å’Œé”é“¾1åŽ»è¥¿è¾¹_èŽ«çµå–µ;
        [UserSetting("P4 Logic Of Marking Teammates With Wyrmfang")]
        public Phase4_Logics_Of_Marking_Teammates_With_Wyrmfang Phase4_Logic_Of_Marking_Teammates_With_Wyrmfang { get; set; }
        [UserSetting("P4 Drawing Duration Of Normal And Delayed Lights (seconds)")]
        public float Phase4_Drawing_Duration_Of_Normal_And_Delayed_Lights { get; set; } = 3f;
        [UserSetting("P4 Colour Of Tidal Light")]
        public ScriptColor Phase4_Colour_Of_Tidal_Light { get; set; } = new() { V4 = new(1f, 1f, 0f, 1f) };
        [UserSetting("P4 Position Before Knockback")]
        public Phase4_Positions_Before_Knockback Phase4_Position_Before_Knockback { get; set; } = Phase4_Positions_Before_Knockback.Normal_æ­£æ”»_èŽ«çµå–µä¸ŽMMW;
        [UserSetting("P4 Logic Of Residue Guidance")]
        public Phase4_Logics_Of_Residue_Guidance Phase4_Logic_Of_Residue_Guidance { get; set; } = Phase4_Logics_Of_Residue_Guidance.According_To_Signs_On_Me_æ ¹æ®æˆ‘èº«ä¸Šçš„ç›®æ ‡æ ‡è®°_èŽ«çµå–µå’ŒMMW;
        [UserSetting("P4 Colour Of Residue Guidance")]
        public ScriptColor Phase4_Colour_Of_Residue_Guidance { get; set; } = new() { V4 = new(1f, 1f, 0f, 1f) };
        [UserSetting("P4 Residue Belongs To Attack1 (Sign Setting)")]
        public Phase4_Relative_Positions_Of_Residues Phase4_Residue_Belongs_To_Attack1 { get; set; } = Phase4_Relative_Positions_Of_Residues.Eastmost_æœ€ä¸œä¾§;
        [UserSetting("P4 Residue Belongs To Attack2 (Sign Setting)")]
        public Phase4_Relative_Positions_Of_Residues Phase4_Residue_Belongs_To_Attack2 { get; set; } = Phase4_Relative_Positions_Of_Residues.About_East_æ¬¡ä¸œä¾§;
        [UserSetting("P4 Residue Belongs To Attack3 (Sign Setting)")]
        public Phase4_Relative_Positions_Of_Residues Phase4_Residue_Belongs_To_Attack3 { get; set; } = Phase4_Relative_Positions_Of_Residues.About_West_æ¬¡è¥¿ä¾§;
        [UserSetting("P4 Residue Belongs To Attack4 (Sign Setting)")]
        public Phase4_Relative_Positions_Of_Residues Phase4_Residue_Belongs_To_Attack4 { get; set; } = Phase4_Relative_Positions_Of_Residues.Westmost_æœ€è¥¿ä¾§;
        [UserSetting("P4 Residue Belongs To Dark Eruption (Debuff Setting)")]
        public Phase4_Relative_Positions_Of_Residues Phase4_Residue_Belongs_To_Dark_Eruption { get; set; } = Phase4_Relative_Positions_Of_Residues.Eastmost_æœ€ä¸œä¾§;
        [UserSetting("P4 Residue Belongs To Unholy Darkness (Debuff Setting)")]
        public Phase4_Relative_Positions_Of_Residues Phase4_Residue_Belongs_To_Unholy_Darkness { get; set; } = Phase4_Relative_Positions_Of_Residues.About_East_æ¬¡ä¸œä¾§;
        [UserSetting("P4 Residue Belongs To Dark Blizzard III (Debuff Setting)")]
        public Phase4_Relative_Positions_Of_Residues Phase4_Residue_Belongs_To_Dark_Blizzard_III { get; set; } = Phase4_Relative_Positions_Of_Residues.About_West_æ¬¡è¥¿ä¾§;
        [UserSetting("P4 Residue Belongs To Dark Water III (Debuff Setting)")]
        public Phase4_Relative_Positions_Of_Residues Phase4_Residue_Belongs_To_Dark_Water_III { get; set; } = Phase4_Relative_Positions_Of_Residues.Westmost_æœ€è¥¿ä¾§;
        [UserSetting("P4 Length Of Drachen Wanderer Hitboxes (meters)")]
        public float Phase4_Length_Of_Drachen_Wanderer_Hitboxes { get; set; } = 1.5f;
        [UserSetting("P4 Colour Of Drachen Wanderer Hitboxes")]
        public ScriptColor Phase4_Colour_Of_Drachen_Wanderer_Hitboxes { get; set; } = new() { V4 = new(1f, 1f, 0f, 1f) };

        [UserSetting("-----Phase 5 Settings----- (No actual meaning for this setting)")]
        public bool _____Phase5_Settings_____ { get; set; } = true;
        [UserSetting("P5 Colour Of Fulgent Blade")]
        public ScriptColor Phase5_Colour_Of_Fulgent_Blade { get; set; } = new() { V4 = new(0, 1f, 1f, 1f) };
        [UserSetting("P5 Colour Of The Current Guidance Step")]
        public ScriptColor Phase5_Colour_Of_The_Current_Guidance_Step { get; set; } = new() { V4 = new(0f, 1f, 0f, 1f) };
        [UserSetting("P5 Colour Of The Next Guidance Step")]
        public ScriptColor Phase5_Colour_Of_The_Next_Guidance_Step { get; set; } = new() { V4 = new(1f, 1f, 0f, 1f) };
        [UserSetting("P5 Colour Of The Boss Central Axis")]
        public ScriptColor Phase5_Colour_Of_The_Boss_Central_Axis { get; set; } = new() { V4 = new(1f, 0f, 0f, 1f) };
        [UserSetting("P5 Boss Faces Players After Fulgent Blade")]
        public bool Phase5_Boss_Faces_Players_After_Fulgent_Blade { get; set; } = true;
        [UserSetting("P5 Wings Dark And Light Strat")]
        public Phase5_Strats_Of_Wings_Dark_And_Light Phase5_Strat_Of_Wings_Dark_And_Light { get; set; } = Phase5_Strats_Of_Wings_Dark_And_Light.Grey9_Brain_Dead_MT_First_Tower_Opposite_ç°ä¹è„‘æ­»æ³•MTä¸€å¡”å¯¹ä¾§_èŽ«çµå–µä¸ŽMMW;
        [UserSetting("P5 Grey9 Brain Dead Strat Branch")]
        public Phase5_Branches_Of_The_Grey9_Brain_Dead_Strat Phase5_Branch_Of_The_Grey9_Brain_Dead_Strat { get; set; } = Phase5_Branches_Of_The_Grey9_Brain_Dead_Strat.Healers_First_Then_Melees_Left_Ranges_Right_å¥¶å¦ˆå…ˆç„¶åŽè¿‘æˆ˜å·¦è¿œç¨‹å³_èŽ«çµå–µ;
        [UserSetting("P5 Reverse Triangle Strat Branch")]
        public Phase5_Branches_Of_The_Reverse_Triangle_Strat Phase5_Branch_Of_The_Reverse_Triangle_Strat { get; set; }
        [UserSetting("P5 Reminder To Provoke During Wings Dark And Light")]
        public bool Phase5_Reminder_To_Provoke_During_Wings_Dark_And_Light { get; set; } = true;
        [UserSetting("P5 Order During Polarizing Strikes")]
        public Phase5_Orders_During_Polarizing_Strikes Phase5_Order_During_Polarizing_Strikes { get; set; } = Phase5_Orders_During_Polarizing_Strikes.Tanks_Melees_Ranges_Healers_å¦å…‹è¿‘æˆ˜è¿œç¨‹å¥¶å¦ˆ_èŽ«çµå–µä¸ŽMMW;

        [UserSetting("-----Developer Settings----- (No actual meaning for this setting)")]
        public bool _____Developer_Settings_____ { get; set; } = true;
        [UserSetting("Enable Developer Mode")]
        public bool Enable_Developer_Mode { get; set; } = false;
        
        #endregion

        #region Variables
        
        int? firstTargetIcon = null;
        int parse=-1;
        volatile bool isInPhase5 = false;
        System.Threading.AutoResetEvent shenaniganSemaphore=new System.Threading.AutoResetEvent(false);

        int P1é›¾é¾™è®¡æ•° = 0;
        readonly Object P1é›¾é¾™è®¡æ•°è¯»å†™é”_AsAConstant = new Object();
        int P1é›¾é¾™è®¡æ•°2 = 0;
        readonly Object P1é›¾é¾™è®¡æ•°2è¯»å†™é”_AsAConstant = new Object();
        List<int> P1é›¾é¾™è®°å½• = new List<int>{0, 0, 0, 0};
        List<MarkType> phase1_markForThePlayersInSafePositions_asAConstant = [
            MarkType.Attack1,
            MarkType.Attack2,
            MarkType.Attack3,
            MarkType.Attack4,
            MarkType.Attack5,
            MarkType.Attack6,
            MarkType.Attack7,
            MarkType.Attack8
        ];
        bool P1é›¾é¾™é›· = false;
        List<int> P1è½¬è½®å¬æŠ“äºº = [0, 0, 0, 0, 0, 0, 0, 0];
        volatile int phase1_timesBurnishedGloryWasCast = 0;
        volatile List<int> phase1_tetheredPlayersDuringFallOfFaith = [];
        volatile bool phase1_isInFallOfFaith = false;
        List<MarkType> phase1_markForTheTetheredPlayer_asAConstant = [
            MarkType.Stop1,
            MarkType.Bind1,
            MarkType.Stop2,
            MarkType.Bind2
        ];
        List<MarkType> phase1_markForTheUntetheredPlayer_asAConstant = [
            MarkType.Attack1,
            MarkType.Attack2,
            MarkType.Attack3,
            MarkType.Attack4
        ];
        volatile int phase1_semaphoreOfMarkingTetheredPlayers = 0;
        volatile int phase1_semaphoreOfShortPrompts = 0;
        volatile int phase1_semaphoreOfDrawing = 0;
        volatile int phase1_semaphoreOfMarkingUntetheredPlayers = 0;
        volatile int phase1_semaphoreOfTheFinalPrompt = 0;
        List<int> P1å¡” = [0, 0, 0, 0];

        volatile string phase2_bossId = "";
        bool P2DDDircle = false;
        volatile List<int> Phase2_Positions_Of_Icicle_Impact = [];
        Vector3 phase2_positionToBeKnockedBack = new Vector3(100, 0, 100);
        System.Threading.AutoResetEvent phase2_semaphoreOfGuidanceBeforeKnockback = new System.Threading.AutoResetEvent(false);
        System.Threading.AutoResetEvent phase2_semaphoreOfGuidanceAfterKnockback = new System.Threading.AutoResetEvent(false);
        volatile int phase2_proteanPositionOfTheColourlessMirror = -1;
        System.Threading.AutoResetEvent phase2_semaphoreTheColourlessMirrorWasConfirmed = new System.Threading.AutoResetEvent(false);
        volatile List<int> phase2_proteanPositionsOfRedMirrors = [];
        System.Threading.AutoResetEvent phase2_semaphoreRedMirrorsWereConfirmed = new System.Threading.AutoResetEvent(false);
        volatile List<int> phase2_playersWithLuminousHammer = [];
        System.Threading.AutoResetEvent phase2_semaphoreLuminousHammerWasConfirmed = new System.Threading.AutoResetEvent(false);
        volatile List<int> phase2_stacksOfLightsteeped = [0, 0, 0, 0, 0, 0, 0, 0];
        volatile bool phase2_writePermissionForLightsteeped = true;
        System.Threading.AutoResetEvent phase2_semaphoreFinalLightsteepedWasConfirmed = new System.Threading.AutoResetEvent(false);

        volatile string phase3_bossId = "";
        List<int> P3FireBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> P3WaterBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> P3ReturnBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> P3Lamp = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> P3LampWise = [0, 0, 0, 0, 0, 0, 0, 0];
        List<int> P3Stack = [0, 0, 0, 0, 0, 0, 0, 0];
        bool P3FloorFireDone = false;
        int P3FloorFire = 0;
        volatile List<Phase3_Types_Of_Dark_Water_III> phase3_typeOfDarkWaterIii = [
            Phase3_Types_Of_Dark_Water_III.NONE,
            Phase3_Types_Of_Dark_Water_III.NONE,
            Phase3_Types_Of_Dark_Water_III.NONE,
            Phase3_Types_Of_Dark_Water_III.NONE,
            Phase3_Types_Of_Dark_Water_III.NONE,
            Phase3_Types_Of_Dark_Water_III.NONE,
            Phase3_Types_Of_Dark_Water_III.NONE,
            Phase3_Types_Of_Dark_Water_III.NONE
        ];
        volatile List<MarkType> phase3_marksOfPlayers = [
            MarkType.Stop1,
            MarkType.Stop1,
            MarkType.Stop1,
            MarkType.Stop1,
            MarkType.Stop1,
            MarkType.Stop1,
            MarkType.Stop1,
            MarkType.Stop1
        ];
        volatile int phase3_numberOfDarkWaterIiiHasBeenProcessed = 0;
        volatile int phase3_numberOfMarksHaveBeenRecorded = 0;
        System.Threading.AutoResetEvent phase3_semaphoreMarksHaveBeenRecorded = new System.Threading.AutoResetEvent(false);
        volatile int phase3_roundOfDarkWaterIii = 0;
        volatile int phase3_rangeSemaphoreOfDarkWaterIii = 0;
        volatile int phase3_guidanceSemaphoreOfDarkWaterIii = 0;
        List<int> phase3_doubleGroup_priority_asAConstant = [2, 3, 0, 1, 4, 5, 6, 7];
        // The priority would be H1 H2 MT OT M1 M2 R1 R2 or H1 H2 MT ST D1 D2 D3 D4 temporarily if the Double Group strat is adopted.
        List<int> phase3_locomotive_priority_asAConstant = [0, 1, 2, 3, 7, 6, 5, 4];
        // The priority would be MT OT H1 H2 R2 R1 M2 M1 or MT ST H1 H2 D4 D3 D2 D1 temporarily if the Locomotive strat is adopted.
        volatile bool phase3_hasConfirmedInitialSafePositions = false;
        Vector3 phase3_doubleGroup_initialSafePositionOfTheLeftGroup = new Vector3(100, 0, 100);
        Vector3 phase3_doubleGroup_initialSafePositionOfTheRightGroup = new Vector3(100, 0, 100);
        Vector3 phase3_doubleGroup_leftPositionToStackOfTheSecondRound = new Vector3(100, 0, 100);
        Vector3 phase3_doubleGroup_rightPositionToStackOfTheSecondRound = new Vector3(100, 0, 100);
        Vector3 phase3_locomotive_initialSafePositionOfTheLeftGroup = new Vector3(100, 0, 100);
        Vector3 phase3_locomotive_initialSafePositionOfTheRightGroup = new Vector3(100, 0, 100);
        Vector3 phase3_locomotive_leftPositionToStackOfTheSecondRound = new Vector3(100, 0, 100);
        Vector3 phase3_locomotive_rightPositionToStackOfTheSecondRound = new Vector3(100, 0, 100);
        Vector3 phase3_moglinMeow_initialSafePositionOfTheLeftGroup = new Vector3(100, 0, 100);
        Vector3 phase3_moglinMeow_initialSafePositionOfTheRightGroup = new Vector3(100, 0, 100);
        Vector3 phase3_moglinMeow_leftPositionToStackOfTheSecondRound = new Vector3(100, 0, 100);
        Vector3 phase3_moglinMeow_rightPositionToStackOfTheSecondRound = new Vector3(100, 0, 100);
        Vector3 phase3_finalPositionOfTheBoss = new Vector3(100, 0, 100);

        ulong P4FragmentId;
        List<int> P4Tether = [-1, -1, -1, -1, -1, -1, -1, -1];
        List<int> P4Stack = [0, 0, 0, 0, 0, 0, 0, 0];
        bool P4TetherDone = false;
        List<int> P4ClawBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        volatile int phase4_numberOfMajorDebuffsHaveBeenCounted = 0;
        readonly Object phase4_readwriteLockOfMajorDebuffCounter_AsAConstant = new Object();
        System.Threading.AutoResetEvent phase4_semaphoreMajorDebuffsWereConfirmed = new System.Threading.AutoResetEvent(false);
        volatile int phase4_numberOfIncidentalDebuffsHaveBeenCounted = 0;
        readonly Object phase4_readwriteLockOfIncidentalDebuffCounter_AsAConstant = new Object();
        System.Threading.AutoResetEvent phase4_semaphoreIncidentalDebuffsWereConfirmed = new System.Threading.AutoResetEvent(false);
        List<MarkType> phase4_markForPlayersWithWyrmfang_asAConstant = [
            MarkType.Attack1,
            MarkType.Attack2,
            MarkType.Attack3,
            MarkType.Attack4
        ];
        List<int> P4OtherBuff = [0, 0, 0, 0, 0, 0, 0, 0];
        volatile List<MarkType> phase4_marksOfPlayersWithWyrmfang = [
            MarkType.Cross,
            MarkType.Cross,
            MarkType.Cross,
            MarkType.Cross,
            MarkType.Cross,
            MarkType.Cross,
            MarkType.Cross,
            MarkType.Cross
        ];
        int P4BlueTether = 0;
        List<Vector3> P4WaterPos = [];
        volatile string phase4_id1OfTheDrachenWanderers = "";
        volatile string phase4_id2OfTheDrachenWanderers = "";
        readonly Object phase4_readwriteLockOfDrachenWandererIds_AsAConstant = new Object();
        volatile int phase4_timesTheWyrmclawDebuffWasRemoved = 0;
        volatile List<ulong> phase4_residueIdsFromEastToWest = [0, 0, 0, 0];
        // The leftmost (0), the about left (1), the about right (2), the rightmost (3) while facing south.
        volatile bool phase4_guidanceOfResiduesHasBeenGenerated = false;
        System.Threading.ManualResetEvent phase4_1_ManualReset = new System.Threading.ManualResetEvent(false);
        int phase4_1_TetherCount = 0;
        private static CrystallizeTime _cry = new();
        private static PriorityDict _pd = new();
        private static List<System.Threading.ManualResetEvent> _events = [.. Enumerable.Range(0, 20).Select(_ => new System.Threading.ManualResetEvent(false))];
        
        volatile string phase5_bossId = "";
        volatile bool phase5_hasAcquiredTheFirstTower = false;
        volatile string phase5_indexOfTheFirstTower = "";
        volatile bool phase5_hasConfirmedTheInitialPosition = false;
        Vector3 phase5_leftSideOfTheSouth_asAConstant = new Vector3(98, 0, 107);
        Vector3 phase5_rightSideOfTheSouth_asAConstant = new Vector3(102, 0, 107);
        Vector3 phase5_leftSideOfTheNortheast_asAConstant = new Vector3(107.06f, 0, 98.23f);
        Vector3 phase5_rightSideOfTheNortheast_asAConstant = new Vector3(105.06f, 0, 94.77f);
        Vector3 phase5_leftSideOfTheNorthwest_asAConstant = new Vector3(94.94f, 0, 94.77f);
        Vector3 phase5_rightSideOfTheNorthwest_asAConstant = new Vector3(92.94f, 0, 98.23f);
        Vector3 phase5_standbyPointBetweenSouthAndNortheast_asAConstant = new Vector3(106.06f, 0, 103.50f);
        Vector3 phase5_standbyPointBetweenSouthAndNorthwest_asAConstant = new Vector3(93.94f, 0, 103.50f);
        Vector3 phase5_standbyPointBetweenNortheastAndNorthwest_asAConstant = new Vector3(100, 0, 93);
        Vector3 phase5_positionToTakeHitsOnTheLeft_asAConstant = new Vector3(95.93f, 0, 104.07f);
        Vector3 phase5_positionToBeCoveredOnTheLeft_asAConstant = new Vector3(93.81f, 0, 106.19f);
        Vector3 phase5_positionToStandbyOnTheLeft_asAConstant = new Vector3(99.24f, 0, 108.72f);
        Vector3 phase5_positionToTakeHitsOnTheRight_asAConstant = new Vector3(104.07f, 0, 104.07f);
        Vector3 phase5_positionToBeCoveredOnTheRight_asAConstant = new Vector3(106.19f, 0, 106.19f);
        Vector3 phase5_positionToStandbyOnTheRight_asAConstant = new Vector3(100.76f, 0, 108.72f);
        // The left and right here refer to the left and right while facing the center of the zone (100,0,100).
        private string Phase = "";
        private Vector2? Point1 = new Vector2(0f, 0f);
        private Vector2? Point2 = new Vector2(0f, 0f);
        private Vector2? Point3 = new Vector2(0f, 0f);
        private Vector2? MiddlePoint = new Vector2(0f, 0f);
        private onPoint? OnPoint = null;
        private int bladeCount = 0;
        //private List<Blade> blades = new List<Blade>();
        private ConcurrentBag<Blade> blades = new ConcurrentBag<Blade>();
        private List<Blade> P1P3Blades = new List<Blade>();
        private List<onPoint> onPoints = new List<onPoint>();
        private List<Vector2?> BladeRoutes;
        private readonly object bladeLock = new object();
        private readonly object drawLock = new object();
        
        #endregion

        #region Enumerations_And_Classes

        public enum Languages_Of_Prompts
        {

            Simplified_Chinese_ç®€ä½“ä¸­æ–‡,
            English_è‹±æ–‡

        }

        public enum Weird_Shenanigans {
            
            Disabled_ä¸å¯ç”¨,
            Astesia_ACR_èŸ¹èŸ¹çš„ACR,
            Res_Gestae_Populi_Romani_II_Bellum_Hannibalicum_ç½—é©¬äººçš„æ•…äº‹2æ±‰å°¼æ‹”æˆ˜çºª,
            Helldivers_ç»åœ°æ½œå…µ,
            Call_Of_Duty_Death_Quotes_ä½¿å‘½å¬å”¤é˜µäº¡åäººåè¨€,
            StarCraft_SCBoy_æ˜Ÿé™…äº‰éœ¸æ˜Ÿé™…è€ç”·å­©
            
        }

        public enum Tanks
        {

            MT,
            OT_ST

        }

        public enum Phase1_Standby_Positions_Of_Utopian_Sky
        {

            Swap_OT_And_M2_äº¤æ¢STä¸ŽD4_èŽ«çµå–µä¸ŽMMW,
            Both_Tanks_Go_Center_åŒTåŽ»ä¸­é—´

        }

        public enum Phase1_Groups_Of_Turn_Of_The_Heavens
        {

            MTOTH1H2_Go_North_MTM1_vary_MTSTH1H2åŽ»åŒ—MTD1æ¢_èŽ«çµå–µä¸ŽMMW,
            MTH1M1R1_Go_North_MTOT_vary_MTH1D1D3åŽ»åŒ—MTSTæ¢,
            MTOTR1R2_Go_North_MTM1_vary_MTSTD3D4åŽ»åŒ—MTD1æ¢_èŽ«çµå–µ

        }

        public enum Phase1_Strats_Of_Fall_Of_Faith
        {

            Single_Line_In_THD_Order_æŒ‰THDé¡ºåºå•æŽ’,
            Single_Line_In_HTD_Order_æŒ‰HTDé¡ºåºå•æŽ’_èŽ«çµå–µ,
            Single_Line_In_H1TDH2_Order_æŒ‰H1TDH2é¡ºåºå•æŽ’,
            Double_Lines_H12MOT_Left_M12R12_Right_åŒæŽ’å·¦H12MSTå³D1234_èŽ«çµå–µä¸ŽMMW,
            Double_Lines_MOTH12_Left_M12R12_Right_åŒæŽ’å·¦MSTH12å³D1234

        }

        public enum Phase1_Orientation_Benchmarks_During_Fall_Of_Faith
        {

            High_Priority_Left_Facing_Due_North_é¢å‘æ­£åŒ—å·¦ä¾§é«˜ä¼˜å…ˆçº§,
            High_Priority_Left_Facing_The_Boss_é¢å‘Bosså·¦ä¾§é«˜ä¼˜å…ˆçº§_èŽ«çµå–µä¸ŽMMW

        }

        public enum Phase1_Strats_Of_Towers
        {

            Completely_Based_On_Priority_å®Œå…¨æ ¹æ®ä¼˜å…ˆçº§_èŽ«çµå–µ,
            Fixed_H1H2R2_Priority_For_Rest_å›ºå®šH1H2D4å‰©ä½™äººä¼˜å…ˆçº§,
            Fixed_H1H2R2_Rest_Fill_Vacancies_å›ºå®šH1H2D4å‰©ä½™äººè¡¥ä½_MMW

        }

        public enum Phase2_Strats_After_Knockback
        {

            Clockwise_One_Group_Counterclockwise_æ€»æ˜¯é¡ºæ—¶é’ˆå•ç»„é€†æ—¶é’ˆ,
            Counterclockwise_One_Group_Clockwise_æ€»æ˜¯é€†æ—¶é’ˆå•ç»„é¡ºæ—¶é’ˆ,
            Clockwise_Both_Groups_Counterclockwise_æ€»æ˜¯é¡ºæ—¶é’ˆåŒç»„é€†æ—¶é’ˆ_èŽ«çµå–µä¸ŽMMW,
            Counterclockwise_Both_Groups_Clockwise_æ€»æ˜¯é€†æ—¶é’ˆåŒç»„é¡ºæ—¶é’ˆ

        }

        public enum Phase2_Strats_Of_Mirror_Mirror
        {

            Melee_Group_Left_Red_è¿‘æˆ˜ç»„åŽ»å·¦çº¢è‰²é•œå­,
            Melee_Group_Right_Red_è¿‘æˆ˜ç»„åŽ»å³çº¢è‰²é•œå­,
            Melee_Group_Closest_Red_Left_If_Same_è¿‘æˆ˜ç»„æœ€è¿‘çº¢è‰²é•œå­è·ç¦»ç›¸åŒåˆ™å·¦,
            Melee_Group_Closest_Red_Right_If_Same_è¿‘æˆ˜ç»„æœ€è¿‘çº¢è‰²é•œå­è·ç¦»ç›¸åŒåˆ™å³_èŽ«çµå–µä¸ŽMMW

        }
        
        public enum Phase2_Initial_Protean_Positions_Of_Light_Rampant
        {

            Supporters_North_MOTH12_For_JPPF_And_L_è“ç»¿å…¨éƒ¨åœ¨åŒ—MSTH12_æ—¥é‡Žå’ŒLå›¢ç”¨,
            Supporters_North_H12MOT_For_JPPF_And_L_è“ç»¿å…¨éƒ¨åœ¨åŒ—H12MST_æ—¥é‡Žå’ŒLå›¢ç”¨,
            Normal_Protean_Tanks_North_East_For_Both_Grey9_å¸¸è§„å…«æ–¹Tåœ¨ä¸œåŒ—_ç°9ç”¨

        }

        public enum Phase2_Strats_Of_Light_Rampant
        {

            Star_Of_David_Japanese_PF_å…­èŠ’æ˜Ÿæ—¥æœé‡Žé˜Ÿæ³•_èŽ«çµå–µä¸ŽMMW,
            New_Grey9_æ–°ç°ä¹æ³•_èŽ«çµå–µä¸ŽMMW,
            Lucrezia_Lå›¢æ³•,
            Obsolete_Old_Grey9_å·²æ·˜æ±°çš„æ—§ç°ä¹æ³•_èŽ«çµå–µ

        }

        public enum Phase3_Strats_Of_The_First_Half
        {

            Moogle_èŽ«å¤åŠ›_èŽ«çµå–µä¸ŽMMW,
            Other_Strats_Are_Work_In_Progress_å…¶ä»–æ”»ç•¥æ­£åœ¨æ–½å·¥ä¸­

        }

        public enum Phase3_Strats_Of_The_Second_Half
        {

            Double_Group_åŒåˆ†ç»„æ³•,
            High_Priority_As_Locomotives_è½¦å¤´ä½Žæ¢æ³•_MMW,
            Moglin_Meow_Or_Baby_Wheelchair_Based_On_Signs_æ ¹æ®ç›®æ ‡æ ‡è®°çš„èŽ«çµå–µæ³•æˆ–å®å®æ¤…æ³•

        }

        public enum Phase3_Branches_Of_The_Double_Group_Strat
        {

            Based_On_Safe_Positions_å®‰å…¨åŒºä¸ºåŸºå‡†_MMW,
            Based_On_The_Second_Apocalypse_ç¬¬äºŒæ¬¡å¯ç¤ºä¸ºåŸºå‡†

        }

        public enum Phase3_Branches_Of_The_Locomotive_Strat
        {

            MT_And_M1_As_Locomotives_MTå’ŒD1ä¸ºè½¦å¤´_MMW,
            Others_As_Locomotives_Chinese_PF_å›½æœé‡Žé˜Ÿäººç¾¤ä¸ºè½¦å¤´

        }

        public enum Phase3_Divisions_Of_The_Zone
        {

            North_To_Southwest_For_The_Left_Group_å·¦ç»„ä»Žæ­£åŒ—åˆ°è¥¿å—_èŽ«çµå–µä¸ŽMMW,
            Northwest_To_South_For_The_Left_Group_å·¦ç»„ä»Žè¥¿åŒ—åˆ°æ­£å—

        }

        public enum Phase3_Types_Of_Dark_Water_III
        {

            LONG,
            MEDIUM,
            SHORT,
            NONE

        }
        
        public enum Phase4_Strats_Of_The_First_Half
        {

            Single_Swap_Baiting_After_å…ˆå•æ¢å†å¼•å¯¼_èŽ«çµå–µä¸ŽMMW,
            Single_Swap_Baiting_First_å…ˆå¼•å¯¼å†å•æ¢,
            Double_Swaps_Baiting_First_å…ˆå¼•å¯¼å†åŒæ¢

        }

        public enum Phase4_Player_Types_To_Be_Marked {
            
            Both_The_Debuffs_Wyrmclaw_And_Wyrmfang_åœ£é¾™çˆªåœ£é¾™ç‰™ä¸¤ç§éƒ½æ ‡è®°,
            Only_Wyrmclaw_The_Red_Debuff_ä»…åœ£é¾™çˆªçº¢è‰²Debuff,
            Only_Wyrmfang_The_Blue_Debuff_ä»…åœ£é¾™ç‰™è“è‰²Debuff
            
        }

        public enum Phase4_Priorities_Of_The_Players_With_Wyrmclaw
        {

            In_THD_Order_æŒ‰THDé¡ºåº_èŽ«çµå–µ,
            In_HTD_Order_æŒ‰HTDé¡ºåº_MMW,
            In_H1TDH2_Order_æŒ‰H1TDH2é¡ºåº

        }

        public enum Phase4_Logics_Of_Marking_Teammates_With_Wyrmclaw {
            
            Ignore1_And_Bind1_Go_West_ç¦æ­¢1å’Œé”é“¾1åŽ»è¥¿è¾¹_èŽ«çµå–µ,
            Ignore1_And_Ignore2_Go_West_ç¦æ­¢1å’Œç¦æ­¢2åŽ»è¥¿è¾¹
            
        }

        public enum Phase4_Logics_Of_Residue_Guidance
        {

            According_To_Signs_On_Me_æ ¹æ®æˆ‘èº«ä¸Šçš„ç›®æ ‡æ ‡è®°_èŽ«çµå–µå’ŒMMW,
            According_To_Debuffs_æ ¹æ®Debuff

        }

        public enum Phase4_Logics_Of_Marking_Teammates_With_Wyrmfang
        {

            According_To_Debuffs_1234_From_East_To_West_æ ¹æ®Debuffä»Žä¸œåˆ°è¥¿1234,
            According_To_Debuffs_1342_From_East_To_West_æ ¹æ®Debuffä»Žä¸œåˆ°è¥¿1342,
            According_To_The_Priority_THD_æ ¹æ®THDä¼˜å…ˆçº§,
            According_To_The_Priority_HTD_æ ¹æ®HTDä¼˜å…ˆçº§,
            According_To_The_Priority_H1TDH2_æ ¹æ®H1TDH2ä¼˜å…ˆçº§

        }

        public enum Phase4_Relative_Positions_Of_Residues
        {

            Eastmost_æœ€ä¸œä¾§,
            About_East_æ¬¡ä¸œä¾§,
            About_West_æ¬¡è¥¿ä¾§,
            Westmost_æœ€è¥¿ä¾§,
            Unknown_æœªçŸ¥

        }

        public enum Phase4_Positions_Before_Knockback
        {

            Normal_æ­£æ”»_èŽ«çµå–µä¸ŽMMW,
            Y_Formation_Japanese_PF_æ—¥æœé‡Žé˜ŸYå­—é˜Ÿå½¢

        }

        public enum Phase5_Strats_Of_Wings_Dark_And_Light
        {

            Grey9_Brain_Dead_MT_First_Tower_Opposite_ç°ä¹è„‘æ­»æ³•MTä¸€å¡”å¯¹ä¾§_èŽ«çµå–µä¸ŽMMW,
            Reverse_Triangle_MT_Baits_In_Towers_å€’ä¸‰è§’æ³•MTåœ¨å¡”ä¸­å¼•å¯¼

        }

        public enum Phase5_Branches_Of_The_Grey9_Brain_Dead_Strat
        {

            Healers_First_Then_Melees_Left_Ranges_Right_å¥¶å¦ˆå…ˆç„¶åŽè¿‘æˆ˜å·¦è¿œç¨‹å³_èŽ«çµå–µ,
            Melees_First_Then_Healers_Left_Ranges_Right_è¿‘æˆ˜å…ˆç„¶åŽå¥¶å¦ˆå·¦è¿œç¨‹å³,
            Healer_First_Then_Melees_Farther_Ranges_Closer_å¥¶å¦ˆå…ˆç„¶åŽè¿‘æˆ˜è¿œè¿œç¨‹è¿‘_MMW

        }
        
        public enum Phase5_Branches_Of_The_Reverse_Triangle_Strat
        {

            Healers_First_Then_Melees_Left_Ranges_Right_å¥¶å¦ˆå…ˆç„¶åŽè¿‘æˆ˜å·¦è¿œç¨‹å³,
            Melees_First_Then_Healers_Left_Ranges_Right_è¿‘æˆ˜å…ˆç„¶åŽå¥¶å¦ˆå·¦è¿œç¨‹å³

        }

        public enum Phase5_Orders_During_Polarizing_Strikes
        {

            Tanks_Melees_Ranges_Healers_å¦å…‹è¿‘æˆ˜è¿œç¨‹å¥¶å¦ˆ_èŽ«çµå–µä¸ŽMMW,
            Tanks_Healers_Melees_Ranges_å¦å…‹å¥¶å¦ˆè¿‘æˆ˜è¿œç¨‹

        }

        public class Blade
        {
            public UInt32 Id { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Rotation { get; set; }
            public Blade(UInt32 id, double x, double y, double rotation)
            {
                Id = id;
                X = x;
                Y = y;
                Rotation = rotation;
            }
        }

        public class onPoint
        {
            public string Name { get; set; }
            public Vector2 OnCoord { get; set; }//      Point
            public Vector2 Coord1 { get; set; }//         1
            public Vector2 Coord2 { get; set; }//     /       \

            //   4|         |2
            public Vector2 Coord3 { get; set; }//     \       /
            public Vector2 Coord4 { get; set; }//         3

            public onPoint(string name, Vector2 onCoord, Vector2 coord1, Vector2 coord2, Vector2 coord3, Vector2 coord4)
            {
                Name = name;
                this.OnCoord = onCoord;
                this.Coord1 = coord1;
                this.Coord2 = coord2;
                this.Coord3 = coord3;
                this.Coord4 = coord4;
            }
        }
        
        #endregion

        #region Initialization
        
        private void resetPoints()
        {
            onPoints.Clear();
            onPoints.Add(new onPoint("A", new Vector2(100, 93), new Vector2(100, 91.5f), new Vector2(101.4f, 92.9f), new Vector2(100, 94.3f), new Vector2(98.6f, 92.9f)));
            onPoints.Add(new onPoint("B", new Vector2(107, 100), new Vector2(108.5f, 100), new Vector2(107, 101.4f), new Vector2(105.6f, 100), new Vector2(107, 98.6f)));
            onPoints.Add(new onPoint("C", new Vector2(100, 107), new Vector2(100, 108.5f), new Vector2(98.6f, 107), new Vector2(100, 105.6f), new Vector2(101.4f, 107.1f)));
            onPoints.Add(new onPoint("D", new Vector2(93, 100), new Vector2(91.5f, 100), new Vector2(93, 98.6f), new Vector2(94.4f, 100), new Vector2(93, 101.4f)));
        }

        public void Init(ScriptAccessory accessory)
        {
            accessory.Method.RemoveDraw(".*");

            if(Phase1_Mark_Players_In_Safe_Positions
               ||
               Phase1_Mark_Players_During_Fall_Of_Faith
               ||
               Phase4_Mark_Players_During_The_Second_Half) {
                
                accessory.Method.MarkClear();
                
            }
            
            parse=1;
            isInPhase5 = false;
            shenaniganSemaphore.Set();

            P1é›¾é¾™è®°å½• = new List<int>{0, 0, 0, 0};
            P1é›¾é¾™è®¡æ•° = 0;
            P1é›¾é¾™è®¡æ•°2 = 0;
            P1è½¬è½®å¬æŠ“äºº = [0, 0, 0, 0, 0, 0, 0, 0];
            phase1_timesBurnishedGloryWasCast = 0;
            phase1_tetheredPlayersDuringFallOfFaith = [];
            phase1_isInFallOfFaith = false;
            phase1_semaphoreOfMarkingTetheredPlayers = 0;
            phase1_semaphoreOfShortPrompts = 0;
            phase1_semaphoreOfDrawing = 0;
            phase1_semaphoreOfMarkingUntetheredPlayers = 0;
            phase1_semaphoreOfTheFinalPrompt = 0;
            P1å¡” = [0, 0, 0, 0];

            phase2_bossId = "";
            Phase2_Positions_Of_Icicle_Impact.Clear();
            phase2_positionToBeKnockedBack = new Vector3(100, 0, 100);
            phase2_semaphoreOfGuidanceBeforeKnockback = new System.Threading.AutoResetEvent(false);
            phase2_semaphoreOfGuidanceAfterKnockback = new System.Threading.AutoResetEvent(false);
            phase2_proteanPositionOfTheColourlessMirror = -1;
            phase2_semaphoreTheColourlessMirrorWasConfirmed = new System.Threading.AutoResetEvent(false);
            phase2_proteanPositionsOfRedMirrors.Clear();
            phase2_semaphoreRedMirrorsWereConfirmed = new System.Threading.AutoResetEvent(false);
            phase2_playersWithLuminousHammer.Clear();
            phase2_semaphoreLuminousHammerWasConfirmed = new System.Threading.AutoResetEvent(false);
            phase2_stacksOfLightsteeped = [0, 0, 0, 0, 0, 0, 0, 0];
            phase2_writePermissionForLightsteeped = true;
            phase2_semaphoreFinalLightsteepedWasConfirmed = new System.Threading.AutoResetEvent(false);

            phase3_bossId = "";
            P3FloorFireDone = false;
            P3Stack = [0, 0, 0, 0, 0, 0, 0, 0];
            phase3_typeOfDarkWaterIii = [
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE
            ];
            phase3_marksOfPlayers = [
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1
            ];
            phase3_numberOfDarkWaterIiiHasBeenProcessed = 0;
            phase3_numberOfMarksHaveBeenRecorded = 0;
            phase3_semaphoreMarksHaveBeenRecorded = new System.Threading.AutoResetEvent(false);
            phase3_roundOfDarkWaterIii = 0;
            phase3_rangeSemaphoreOfDarkWaterIii = 0;
            phase3_guidanceSemaphoreOfDarkWaterIii = 0;
            phase3_hasConfirmedInitialSafePositions = false;
            phase3_doubleGroup_initialSafePositionOfTheLeftGroup = new Vector3(100, 0, 100);
            phase3_doubleGroup_initialSafePositionOfTheRightGroup = new Vector3(100, 0, 100);
            phase3_doubleGroup_leftPositionToStackOfTheSecondRound = new Vector3(100, 0, 100);
            phase3_doubleGroup_rightPositionToStackOfTheSecondRound = new Vector3(100, 0, 100);
            phase3_locomotive_initialSafePositionOfTheLeftGroup = new Vector3(100, 0, 100);
            phase3_locomotive_initialSafePositionOfTheRightGroup = new Vector3(100, 0, 100);
            phase3_locomotive_leftPositionToStackOfTheSecondRound = new Vector3(100, 0, 100);
            phase3_locomotive_rightPositionToStackOfTheSecondRound = new Vector3(100, 0, 100);
            phase3_moglinMeow_initialSafePositionOfTheLeftGroup = new Vector3(100, 0, 100);
            phase3_moglinMeow_initialSafePositionOfTheRightGroup = new Vector3(100, 0, 100);
            phase3_moglinMeow_leftPositionToStackOfTheSecondRound = new Vector3(100, 0, 100);
            phase3_moglinMeow_rightPositionToStackOfTheSecondRound = new Vector3(100, 0, 100);
            phase3_finalPositionOfTheBoss = new Vector3(100, 0, 100);

            P4FragmentId=0;
            P4Tether=[-1,-1,-1,-1,-1,-1,-1,-1];
            P4Stack=[0,0,0,0,0,0,0,0];
            P4TetherDone=false;
            P4ClawBuff=[0,0,0,0,0,0,0,0];
            phase4_numberOfMajorDebuffsHaveBeenCounted = 0;
            phase4_semaphoreMajorDebuffsWereConfirmed = new System.Threading.AutoResetEvent(false);
            phase4_numberOfIncidentalDebuffsHaveBeenCounted = 0;
            phase4_semaphoreIncidentalDebuffsWereConfirmed = new System.Threading.AutoResetEvent(false);
            P4OtherBuff=[0,0,0,0,0,0,0,0];
            phase4_marksOfPlayersWithWyrmfang = [
                MarkType.Cross,
                MarkType.Cross,
                MarkType.Cross,
                MarkType.Cross,
                MarkType.Cross,
                MarkType.Cross,
                MarkType.Cross,
                MarkType.Cross
            ];
            P4BlueTether=0;
            P4WaterPos=[];
            phase4_id1OfTheDrachenWanderers = "";
            phase4_id2OfTheDrachenWanderers = "";
            phase4_timesTheWyrmclawDebuffWasRemoved = 0;
            phase4_residueIdsFromEastToWest = [0, 0, 0, 0];
            phase4_guidanceOfResiduesHasBeenGenerated = false;
            phase4_1_ManualReset=new System.Threading.ManualResetEvent(false);
            phase4_1_TetherCount=0;
            // It's not necessary to initialize the static variables... right?

            phase5_bossId = "";
            phase5_hasAcquiredTheFirstTower = false;
            phase5_indexOfTheFirstTower = "";
            phase5_hasConfirmedTheInitialPosition = false;
            blades.Clear();
            P1P3Blades.Clear();
            BladeRoutes = Enumerable.Repeat<Vector2?>(null, 7).ToList();
            resetPoints();//Initialize fulgent blade coordinates
        }
        
        #endregion
        
        #region Weird_Shenanigans
        
        [ScriptMethod(name:"Weird Shenanigans",
            eventType:EventTypeEnum.AddCombatant,
            eventCondition:["DataId:9020"],
            suppress:15000,
            userControl:false)]

        public void Weird_Shenanigans_æžæ€ª(Event @event, ScriptAccessory accessory) {

            shenaniganSemaphore.WaitOne();

            System.Threading.Thread.MemoryBarrier();

            System.Threading.Thread.Sleep(3500);
            
            System.Threading.Thread.MemoryBarrier();

            if(Weird_Shenanigan==Weird_Shenanigans.Disabled_ä¸å¯ç”¨) {
                
                shenaniganSemaphore=new System.Threading.AutoResetEvent(false);

                return;

            }
            
            System.Threading.Thread.MemoryBarrier();

            System.Random seed=new System.Random();
            string prompt="";

            if(Weird_Shenanigan==Weird_Shenanigans.Astesia_ACR_èŸ¹èŸ¹çš„ACR) {
                
                int randomNumber=seed.Next(1,101);

                if(randomNumber<=25) {

                    if(Language_Of_Prompts==Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡) {

                        prompt="Welcome to Astesia The Piggy's ACR!";

                    }
                    
                    if(Language_Of_Prompts==Languages_Of_Prompts.English_è‹±æ–‡) {

                        prompt="You're now running Astesia The Piggy's ACR!";

                    }

                }

                else {
                    
                    if(Language_Of_Prompts==Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡) {

                        prompt="Welcome to Astesia's ACR!";

                    }
                    
                    if(Language_Of_Prompts==Languages_Of_Prompts.English_è‹±æ–‡) {

                        prompt="You're now running Astesia's ACR!";

                    }
                    
                }

            }
            
            if(Weird_Shenanigan==Weird_Shenanigans.Res_Gestae_Populi_Romani_II_Bellum_Hannibalicum_ç½—é©¬äººçš„æ•…äº‹2æ±‰å°¼æ‹”æˆ˜çºª) {
                
                List<string> englishContents=[
                    "The First Punic War:\nRome and Carthage clashed in their first large-scale land and naval war over the control of Sicily. In its naval debut, Rome nearly annihilated the Carthaginian fleet. Ultimately, Rome emerged victorious and seized Sicily.",
                    "After the First Punic War:\nCarthage shifted its focus to expanding into Spain to compensate for its losses. During this period, Hannibal, the son of General Hamilcar from the First Punic War, made his legendary entrance onto the stage of history.",
                    "Early Phase of the Second Punic War:\nCarthage initiated the Second Punic War. Hannibal led his army through Gaul and over the Alps in a miraculous feat, inflicting devastating defeats on Rome, including the complete annihilation of Roman forces at the Battle of Cannae.",
                    "Middle Phase of the Second Punic War Part I:\nAlthough Hannibal won victory after victory in Italy, he failed to capture Rome. Meanwhile, Romeâ€™s counteroffensive in Spain was crushed. At this moment of existential crisis for the Republic, Scipio volunteered before the Senate â€” the legendary Roman general stepped into the spotlight.",
                    "Middle Phase of the Second Punic War Part II:\nScipio won a series of brilliant victories in Spain, defeating two Carthaginian armies despite being outnumbered. Hannibalâ€™s reinforcements entering Italy were intercepted and annihilated. Rome regained control over all major cities in southern Italy.",
                    "Late Phase of the Second Punic War:\nScipio landed in North Africa and took control of Numidia. The Carthaginian elders recalled Hannibal home. In the epic Battle of Zama, the two legendary generals faced off, and Scipio used Hannibalâ€™s own tactics to decisively defeat him. Rome triumphed completely.",
                    "After the Punic Wars:\nScipio was forced to resign and retire due to political attacks by his rival Cato. He died shortly after, lamenting: \"Ungrateful country, you won't even have my bones\". Hannibal fled to the Hellenistic Phoenician cities in Greece and eventually took poison to end his life in Asia Minor before being cornered by Roman pursuers.",
                    "The Fall of Macedonia:\nRome defeated the Kingdom of Macedonia in the Third Macedonian War and dissolved it, bringing Greece under Roman control and achieving dominance over the Mediterranean.",
                    "The Fall of Carthage:\nRome launched the Third Punic War. Carthage was captured and utterly destroyed. The Carthaginian state ceased to exist. The Romans, gazing over the Mediterranean, left behind a proud victorâ€™s declaration: \"Mare Nostrum (Our Sea)\"."
                ];
                
                int randomNumber=seed.Next(0,9);

                if(Language_Of_Prompts==Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡) {

                    // prompt=chineseContents[randomNumber]; (Chinese contents omitted for brevity)

                }
                    
                if(Language_Of_Prompts==Languages_Of_Prompts.English_è‹±æ–‡) {

                    prompt=englishContents[randomNumber];
                    
                }

            }
            
            if(Weird_Shenanigan==Weird_Shenanigans.Helldivers_ç»åœ°æ½œå…µ) {

                int randomNumber=seed.Next(1,101);

                if(randomNumber<=10) {

                    prompt="Here? Here! Here? What about here? Here? Here! Here! Here? What about here?\nHellpod launch suspended.";

                }

                else {
                    
                    List<string> systemNames=[
                        "Malevelon Creek",
                        "Meridian",
                        "Turing",
                        "Angel's Venture",
                        "Hellmire", // 5
                        "Cyberstan",
                        "Calypso",
                        "Moradesh",
                        "Fenrir III",
                        "Chort Bay", //10
                        "Marfark",
                        "Omicron",
                        "Vernen Wells",
                        "Genesis Prime",
                        "Mog" //15
                    ];
                    
                    int randomNumber2=seed.Next(0,15);

                    prompt=$"Initiating FTL Jump to, the {systemNames[randomNumber2]} system.\nFTL Jump successful.\nHellpods primed.\nMission coordinates locked.";
                    
                }

            }
            
            if(Weird_Shenanigan==Weird_Shenanigans.Call_Of_Duty_Death_Quotes_ä½¿å‘½å¬å”¤é˜µäº¡åäººåè¨€) {
                
                List<string> englishContents=[
                    "Del Giordano le rive saluta, di Sionne le torri atterrate...",
                    "Through the graves the wind is blowing.",
                    "The enslaved were not bricks in your road, and their lives were not chapters in your redemptive history.",
                    "Thou hast made us for thyself, O Lord, and our heart is restless until it finds its rest in thee.",
                    "No! I'm alive! I will live forever! I have in my heart what does not die!",
                    "The living denied a table; the dead get a whole coffin.",
                    "What was born by the sword shall die by the sword.",
                    "Injustice anywhere is a threat to justice everywhere.",
                    "I die without seeing the dawn brighten over my native land.",
                    "I entered a kind world and loved it wholeheartedly. I leave in an evil one and have nothing to say by way of farewells.",
                    "You cannot nurture a man with pain, nor can you feed him with anger.",
                    "\"Hemos pasado!\"",
                    "The Banteng has been led to slaughter - and the villagers feast on its remnants.",
                    "Those who wear the shirt of fire will realize it burns as much as it warms.",
                    "What is built on sand sooner or later would tumble down.",
                    "A faithful man shall abound with blessings.",
                    "She smiled sadly, as she flew into the night.",
                    "Only in death does duty end.",
                    "The end may justify the means as long as there is something that justifies the end.",
                    "Sing your death song and die like a hero going home.",
                    "The mutineers ride into the night.",
                    "The specter of homicidal violence has appeared in history whenever it was believed that the hypocritical respect for formalities could replace the obedience of moral obligations.",
                    "Nothing more cruel and inhuman than a war. Nothing more desirable than peace. But peace has its causes, it is an effect. The effect of respect for mutual rights.",
                    "One by one the righteous fell, and the ills of ignorance permeated.",
                    "They defended the grains of sand in the desert to the last drop of their blood.",
                    "All history is man's efforts to realise ideals.\n- Ã‰amon de Valera, 1929",
                    "Let us dedicate ourselves to what the Greeks wrote so many years ago: to tame the savageness of man and make gentle the life of this world.\n- Robert F. Kennedy, 1968",
                    "Yesterday is not ours to recover, but tomorrow is ours to win or lose.\n- Lyndon B. Johnson, 1964",
                    "The end of hope is the beginning of death.\n- Charles de Gaulle, 1945",
                    "The day I leave the power, inside my pockets will only be dust.\n- Antonio de Oliveira Salazar, 1968",
                    "When smashing monuments, save the pedestals. They always come in handy.\n- StanisÅ‚aw Jerzy Lec, 1957",
                    "Fear not the path of truth for the lack of people walking on it.\n- Robert F. Kennedy, 1968",
                    "The rocket worked perfectly, except for landing on the wrong planet.\n- Wernher von Braun upon the first V-2 hitting London, 1944",
                    "A man is not finished when he's defeated. He's finished when he quits.\n- Richard Nixon, 1962",
                    "Do not pray for easy lives, pray to be stronger men.\n- John F. Kennedy, 1963",
                    "Nature does not know extinction, only transformation.\n- Wernher von Braun, 1962",
                    "The optimist thinks this is the best of all possible worlds. The pessimist fears it is true.\n- James Branch Cabell, The Silver Stallion, 1926",
                    "One seldom recognizes the devil when he is putting his hand on your shoulder.\n- Albert Speer, 1972",
                    "Laws are silent in times of war.\n- Marcus Tullius Cicero, 52 BC",
                    "They don't ask much of you. They only want you to hate the things you love and to love the things you despise.\n- Boris Pasternak, 1960",
                    "Most economic fallacies derive from the tendency to assume that there is a fixed pie, that one party can gain only at the expense of another.\n- Milton Friedman, 1980",
                    "There are three kinds of lies: lies, damned lies, and statistics.\n- Mark Twain, 1907",
                    "Bite us once, shame on the dog; bite us repeatedly, shame on us for allowing it.\n- Phyllis Schlafly, 1995",
                    "I know not with what weapons World War III will be fought, but World War IV will be fought with sticks and stones.\n- Albert Einstein, 1949",
                    "You can believe in Feng Shui if you want, but ultimately people control their own fate.\n- Li Ka-shing, 1969",
                    "I believe it is a big mistake to think that money is the only way to compensate a person for his work. People need money, but they also want to be happy in their work and proud of it.\n- Morita Akio, 1966",
                    "A good reputation for yourself and your company is an invaluable asset not reflected in the balance sheets.\n- Li Ka-shing, 1967",
                    "Knowledge is your real companion, your life long companion, not fortune. Fortune can disappear.\n- Stanley Ho, 1966",
                    "People sometimes say: \"we are in a society that is all rotten, all dishonest.\" That is not true. There are still so many good people, so many honest people.\n- John Paul I, 1978",
                    "Half the confusion in the world comes from not knowing how little we need.\n- Admiral Richard E. Byrd on his time in Antarctica, 1935"
                ];
                
                int randomNumber=seed.Next(0,50);

                if(Language_Of_Prompts==Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡) {

                    // prompt=chineseContents[randomNumber]; (Chinese contents omitted for brevity)

                }
                    
                if(Language_Of_Prompts==Languages_Of_Prompts.English_è‹±æ–‡) {

                    prompt=englishContents[randomNumber];
                    
                }

            }
            
            if(Weird_Shenanigan==Weird_Shenanigans.StarCraft_SCBoy_æ˜Ÿé™…äº‰éœ¸æ˜Ÿé™…è€ç”·å­©) {
                
                List<string> contents=[
                    "Hey, your ally's base is being overrun!",
                    "Your buddy is getting destroyed, aren't you going to help?",
                    "Your ally is under attack, and you're just watching?",
                    "The base is under attack! Better check it out!",
                    "The base is getting hit! Time for a base trade!", // 5
                    "You can't warp in without a power field!",
                    "Nuke incoming! GG!",
                    "See that red dot? That's a nuke!",
                    "The enemy doesn't want to talk, they just drop a tactical nuke on you.",
                    "Base upgrade complete. That feels good!", // 10
                    "Your command center just got an upgrade!",
                    "Five! Four! Three! Two! One! GG!",
                    "This upgrade is critical and timely.",
                    "Oh, fighting? Time to show off those micro skills!",
                    "Your forces are taking massive damage!", // 15
                    "Your forces are engaging. What?!",
                    "Hey hey, the enemy is attacking your units, buddy!",
                    "Your precious protoss warriors are about to die!",
                    "Your protoss forces are crying in agony!",
                    "The enemy is using pesticide on you!", // 20
                    "Your zerg swarm just got wiped out!",
                    "Pausing the game.",
                    "Alright, I'm back!",
                    "Feeling the gas being depleted...",
                    "Something's in the way, clear the area. Time to crank up the APM!", // 25
                    "You can't land there, what are you thinking?",
                    "Hey, you're supply capped! Time to push!",
                    "Supply capped? Just F2 A-move!",
                    "Mineral field depleted, do you have an expansion?",
                    "Zerg buildings must be placed on creep. You can't even find creep, can you?", // 30
                    "I can't stand this, how can you build without creep? Guess!",
                    "Not enough supply. Your unit production is insane!",
                    "Warp in more pylons. You could try a proxy pylon!",
                    "Nuke ready, go kill them!",
                    "Not enough minerals. Pay attention to your spending!", // 35
                    "Oh, out of minerals! Tough times without money!",
                    "So sneaky, stealing our SCVs!",
                    "Why are you always hitting our SCVs? You're asking for it!",
                    "Your SCV is under attack! They're not giving you any breathing room!",
                    "They're focus-firing your probe, things aren't looking good!" // 40
                ];
                
                int randomNumber=seed.Next(0,40);

                prompt=contents[randomNumber];

            }

            if(!prompt.Equals("")) {

                if(Enable_Text_Prompts) {
                    
                    accessory.Method.TextInfo(prompt,11500);
                    
                }

                if(Enable_Vanilla_TTS||Enable_Daily_Routines_TTS) {
                    
                    accessory.TTS(prompt,Enable_Vanilla_TTS,Enable_Daily_Routines_TTS);
                    
                }
                
            }

            System.Threading.Thread.MemoryBarrier();

            shenaniganSemaphore=new System.Threading.AutoResetEvent(false);

        }
        
        #endregion

        #region Phase_1

        [ScriptMethod(name: "----- Phase 1 ----- (No actual meaning for this toggle)",
            eventType: EventTypeEnum.NpcYell,
            eventCondition: ["Give me your tired",
                            "ç»™æˆ‘ä½ ä»¬ç–²å€¦çš„äºº"])]

        public void Phase1_Placeholder(Event @event, ScriptAccessory accessory) { }

        [ScriptMethod(name: "P1_UtopianSky_BaitingCone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4014[48])|40329|40330)$"])]
        public void P1_UtopianSky_BaitingCone(Event @event, ScriptAccessory accessory)
        {
            if (parse!=1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            foreach (var pm in accessory.Data.PartyList)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_UtopianSky_BaitingCone";
                dp.Scale = new(60);
                dp.Radian = float.Pi / 8;
                dp.Owner = sid;
                dp.TargetObject = pm;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 7000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }

        }
        [ScriptMethod(name: "P1_UtopianSky_SubsequentCone", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(40145)$", "TargetIndex:1"])]
        public void P1_UtopianSky_SubsequentCone(Event @event, ScriptAccessory accessory)
        {
            var dur = 2000;
            if (parse!=1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!float.TryParse(@event["SourceRotation"], out var rot)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_UtopianSky_SubsequentCone1";
            dp.Scale = new(60);
            dp.FixRotation = true;
            dp.Rotation = rot;
            dp.Radian = float.Pi / 8;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_UtopianSky_SubsequentCone2";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 8;
            dp.FixRotation = true;
            dp.Rotation = rot + float.Pi / -8;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 2000;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_UtopianSky_SubsequentCone3";
            dp.Scale = new(60);
            dp.FixRotation = true;
            dp.Rotation = rot + float.Pi / -4;
            dp.Radian = float.Pi / 8;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 4000;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }
        [ScriptMethod(name: "P1_UtopianSky_SpreadStack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4014[48])|40329|40330)$"])]
        public void P1_UtopianSky_SpreadStack(Event @event, ScriptAccessory accessory)
        {
            if (parse!=1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            string prompt = "";

            if (@event["ActionId"] == "40148" || @event["ActionId"] == "40330")
            {
                foreach (var pm in accessory.Data.PartyList)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_UtopianSky_Spread";
                    dp.Scale = new(6);
                    dp.Owner = pm;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 5000;
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    prompt = "åˆ†æ•£";

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    prompt = "Spread";

                }

            }
            else
            {
                int[] group = [6, 7, 4, 5, 2, 3, 0, 1];
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                for (int i = 4; i <= 7; ++i)
                {
                    // The drawing owners here were adjusted a bit by Cicero.
                    // Here's an interesting fact - the action Sinsmoke (stack) will always target DPS.

                    var ismygroup = myindex == i || group[i] == myindex;

                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_UtopianSky_Stack";
                    dp.Scale = new(6);
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = ismygroup ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    dp.Delay = 5000;
                    dp.DestoryAt = 4000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    prompt = "åˆ†æ‘Š";

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    prompt = "Stack";

                }

            }

            System.Threading.Thread.Sleep(5000);

            if (!prompt.Equals(""))
            {

                if (Enable_Text_Prompts)
                {

                    accessory.Method.TextInfo(prompt, 1500);

                }

                accessory.TTS($"{prompt}", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

            }

        }
        [ScriptMethod(name: "P1_UtopianSky_BaitingPosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4014[48])|40329|40330)$"])]
        public void P1_UtopianSky_BaitingPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse!=1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var spread = @event["ActionId"] == "40148" || @event["ActionId"] == "40330";
            var rot8 = myindex switch
            {
                0 => 0,
                1 => 2,
                2 => 6,
                3 => 4,
                4 => 5,
                5 => 3,
                6 => 7,
                7 => 1,
                _ => 0,
            };
            var outPoint = spread && (myindex == 2 || myindex == 3 || myindex == 6 || myindex == 7);
            var mPosEnd = RotatePoint(outPoint ? new(100, 0, 90) : new(100, 0, 95), new(100, 0, 100), float.Pi / 4 * rot8);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_UtopianSky_BaitingPosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = mPosEnd;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        }
        [ScriptMethod(name: "P1_TankBusterBuffExplosion", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4166"])]
        public void P1_TankBusterBuffExplosion(Event @event, ScriptAccessory accessory)
        {
            if (parse!=1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (!int.TryParse(@event["DurationMilliseconds"], out var dur)) return;
            string prompt = "";

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_TankBusterBuffExplosion1";
            dp.Scale = new(10);
            dp.Owner = tid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = dur - 5000;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_TankBusterBuffExplosion2";
            dp.Scale = new(10);
            dp.Owner = tid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.CentreOrderIndex = 1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = dur - 5000;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            if (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 0
               ||
               accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 1)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    prompt = "ç¨å¾®é è¿‘å¦ä¸€ä¸ªT";

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    prompt = "Get slightly closer to another tank";

                }

            }

            if (2 <= accessory.Data.PartyList.IndexOf(accessory.Data.Me)
               &&
               accessory.Data.PartyList.IndexOf(accessory.Data.Me) <= 7)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    prompt = "è¿œç¦»åŒT";

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    prompt = "Stay away from both tanks";

                }

            }

            System.Threading.Thread.Sleep(dur - 5000);

            if (!prompt.Equals(""))
            {

                if (Enable_Text_Prompts)
                {

                    accessory.Method.TextInfo(prompt, 1500);

                }

                accessory.TTS($"{prompt}", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

            }

        }
        [ScriptMethod(name: "P1_UtopianSky_RecordPositions", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40158)$"], userControl: false)]
        public void P1_UtopianSky_RecordPositions(Event @event, ScriptAccessory accessory)
        {
            if (parse!=1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            
            KodakkuAssist.Data.IGameObject? obj=null;
            do {
                ++sid;
                obj=accessory.Data.Objects.SearchByEntityId((uint)sid);
            } while(obj==null);
            
            var dir8 = PositionTo8Dir(obj.Position, new(100, 0, 100));
            P1é›¾é¾™è®°å½•[dir8 % 4] = 1;
        }
        [ScriptMethod(name: "P1_UtopianSky_RecordThunderFire", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4015[45])$"], userControl: false)]
        public void P1_UtopianSky_RecordThunderFire(Event @event, ScriptAccessory accessory)
        {
            if (parse!=1) return;
            P1é›¾é¾™é›· = (@event["ActionId"] == "40155");
        }
        [ScriptMethod(name: "P1_UtopianSky_Range", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40158)$"])]
        public void P1_UtopianSky_Range(Event @event, ScriptAccessory accessory)
        {
            if (parse!=1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            
            KodakkuAssist.Data.IGameObject? obj=null;
            do {
                ++sid;
                obj=accessory.Data.Objects.SearchByEntityId((uint)sid);
            } while(obj==null);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_UtopianSky_Range";
            dp.Scale = new(16, 50);
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P1_UtopianSky_SpreadStack", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4015[45])$"])]
        public void P1_UtopianSky_SpreadStack(Event @event, ScriptAccessory accessory)
        {
            if (parse!=1) return;
            string prompt = "";

            if (@event["ActionId"] == "40155")
            {
                foreach (var pm in accessory.Data.PartyList)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_UtopianSky_Spread";
                    dp.Scale = new(5);
                    dp.Owner = pm;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.Delay = 10000;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    prompt = "åˆ†æ•£";

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    prompt = "Spread";

                }

            }
            else
            {
                List<int> h1group = [0, 2, 4, 6];
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

                var isH1group = h1group.Contains(myindex);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_UtopianSky_Stack1";
                dp.Scale = new(6);
                dp.Owner = accessory.Data.PartyList[2];
                dp.Color = isH1group ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                dp.Delay = 10000;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P1_UtopianSky_Stack2";
                dp.Scale = new(6);
                dp.Owner = accessory.Data.PartyList[3];
                dp.Color = !isH1group ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                dp.Delay = 10000;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    prompt = "åˆ†æ‘Š";

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    prompt = "Stack";

                }

            }

            System.Threading.Thread.Sleep(10000);

            if (!prompt.Equals(""))
            {

                if (Enable_Text_Prompts)
                {

                    accessory.Method.TextInfo(prompt, 1500);

                }

                accessory.TTS($"{prompt}", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

            }

        }

        [ScriptMethod(name: "Phase1 Standby Position Of Utopian Sky",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(4015[45])$"])]

        public void Phase1_Standby_Position_Of_Utopian_Sky_ä¹å›­ç»æŠ€å¾…æœºä½ç½®(Event @event, ScriptAccessory accessory)
        {

            if (parse!=1)
            {

                return;

            }

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            if (Phase1_Standby_Position_Of_Utopian_Sky == Phase1_Standby_Positions_Of_Utopian_Sky.Swap_OT_And_M2_äº¤æ¢STä¸ŽD4_èŽ«çµå–µä¸ŽMMW)
            {

                int rotationMultiplier = myIndex switch
                {
                    0 => 0,
                    1 => 1,
                    2 => 6,
                    3 => 4,
                    4 => 5,
                    5 => 3,
                    6 => 7,
                    7 => 2
                };

                var myPosition = RotatePoint(new(100, 0, 81), new(100, 0, 100), float.Pi / 4 * rotationMultiplier);

                if (myIndex == 0)
                {

                    myPosition = RotatePoint(myPosition, new(100, 0, 100), float.Pi / 72);

                }

                if (myIndex == 1)
                {

                    myPosition = RotatePoint(myPosition, new(100, 0, 100), -(float.Pi / 72));

                }

                if (myIndex == 6)
                {

                    myPosition = RotatePoint(myPosition, new(100, 0, 100), -(float.Pi / 36));

                }

                if (myIndex == 7)
                {

                    myPosition = RotatePoint(myPosition, new(100, 0, 100), float.Pi / 36);

                }

                var currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase1_Standby_Position_Of_Utopian_Sky";
                currentProperty.Scale = new(2);
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = myPosition;
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.DestoryAt = 9000;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            }

            if (Phase1_Standby_Position_Of_Utopian_Sky == Phase1_Standby_Positions_Of_Utopian_Sky.Both_Tanks_Go_Center_åŒTåŽ»ä¸­é—´)
            {

                var myPosition = new Vector3(100, 0, 100);

                if (myIndex == 0)
                {

                    myPosition = new Vector3(100f, 0f, 94.5f);

                }

                if (myIndex == 1)
                {

                    myPosition = new Vector3(100f, 0f, 105.5f);

                }

                if (2 <= myIndex && myIndex <= 7)
                {

                    int rotationMultiplier = myIndex switch
                    {
                        2 => 6,
                        3 => 2,
                        4 => 5,
                        5 => 3,
                        6 => 7,
                        7 => 1
                    };

                    myPosition = RotatePoint(new(100, 0, 81), new(100, 0, 100), float.Pi / 4 * rotationMultiplier);

                }

                if (myPosition.Equals(new Vector3(100, 0, 100)))
                {

                    return;

                }

                var currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase1_Standby_Position_Of_Utopian_Sky";
                currentProperty.Scale = new(2);
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = myPosition;
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.DestoryAt = 9000;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            }

        }

        [ScriptMethod(name: "P1_UtopianSky_SafePosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40158)$"])]
        public void P1_UtopianSky_SafePosition(Event @event, ScriptAccessory accessory)
        {
            if (parse!=1) return;

            lock (P1é›¾é¾™è®¡æ•°è¯»å†™é”_AsAConstant)
            {
                P1é›¾é¾™è®¡æ•°++;
                if (P1é›¾é¾™è®¡æ•° != 3) return;
                Task.Delay(334).ContinueWith(t =>
                {
                    if (!P1é›¾é¾™é›·)
                    {
                        var safeDir = P1é›¾é¾™è®°å½•.IndexOf(0);
                        List<int> h1group = [0, 2, 4, 6];
                        var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                        var isH1group = h1group.Contains(myindex);
                        var rot8 = safeDir switch
                        {
                            0 => isH1group ? 0 : 4,
                            1 => isH1group ? 5 : 1,
                            2 => isH1group ? 6 : 2,
                            3 => isH1group ? 7 : 3,
                            _ => 0
                        };
                        var mPosEnd = RotatePoint(new(100, 0, 84), new(100, 0, 100), float.Pi / 4 * rot8);

                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_UtopianSky_StackPosition";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = mPosEnd;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 9000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                    else
                    {
                        var safeDir = P1é›¾é¾™è®°å½•.IndexOf(0);
                        List<int> h1group = [0, 2, 4, 6];
                        var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                        var isH1group = h1group.Contains(myindex);
                        Vector3 p1 = new(100.0f, 0, 88.0f);
                        Vector3 p2 = new(100.0f, 0, 80.5f);
                        Vector3 p3 = new(106.5f, 0, 81.5f);
                        Vector3 p4 = new(093.5f, 0, 81.5f);
                        var rot8 = safeDir switch
                        {
                            0 => isH1group ? 0 : 4,
                            1 => isH1group ? 5 : 1,
                            2 => isH1group ? 6 : 2,
                            3 => isH1group ? 7 : 3,
                            _ => 0
                        };
                        var myPosA = myindex switch
                        {
                            0 => p2,
                            1 => p2,
                            2 => p1,
                            3 => p1,
                            4 => p3,
                            5 => p3,
                            6 => p4,
                            7 => p4,
                            _ => p1,
                        };
                        var mPosEnd = RotatePoint(myPosA, new(100, 0, 100), float.Pi / 4 * rot8);

                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_UtopianSky_SpreadPosition";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = mPosEnd;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 9000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                });

            }

        }

        [ScriptMethod(name: "Phase1 Mark Players In Safe Positions",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40158"],
            userControl: false)]

        public void Phase1_Mark_Players_In_Safe_Positions_æ ‡è®°åœ¨å®‰å…¨ä½ç½®çš„çŽ©å®¶(Event @event, ScriptAccessory accessory)
        {

            if (parse!=1)
            {

                return;

            }

            if (!Phase1_Mark_Players_In_Safe_Positions)
            {

                return;

            }

            lock (P1é›¾é¾™è®¡æ•°2è¯»å†™é”_AsAConstant)
            {

                ++P1é›¾é¾™è®¡æ•°2;

                System.Threading.Thread.MemoryBarrier();

                if (P1é›¾é¾™è®¡æ•°2 != 3)
                {

                    return;

                }

                Task.Delay(334).ContinueWith(t =>
                {
                    // I know this is not a thread safe practice, but I'm just too lazy to rework it in a thread safe way. Please forgive me :(
                    // And by the way, If something really goes wrong here, it probably indicates that the frame rate of the current user is below 3 FPS.

                    int safePositions = P1é›¾é¾™è®°å½•.IndexOf(0);
                    List<int> temporaryOrder = [0, 1, 2, 3, 4, 5, 6, 7];
                    string debugOutput = "";

                    if (Phase1_Standby_Position_Of_Utopian_Sky == Phase1_Standby_Positions_Of_Utopian_Sky.Swap_OT_And_M2_äº¤æ¢STä¸ŽD4_èŽ«çµå–µä¸ŽMMW)
                    {

                        temporaryOrder = [0, 1, 7, 5, 3, 4, 2, 6];

                    }

                    if (Phase1_Standby_Position_Of_Utopian_Sky == Phase1_Standby_Positions_Of_Utopian_Sky.Both_Tanks_Go_Center_åŒTåŽ»ä¸­é—´)
                    {

                        temporaryOrder = [0, 7, 3, 5, 1, 4, 2, 6];

                    }

                    for (int i = 0, j = 0; i < temporaryOrder.Count; ++i)
                    {

                        var currentObject = accessory.Data.Objects.SearchById(accessory.Data.PartyList[temporaryOrder[i]]);

                        if (currentObject != null)
                        {

                            if (PositionTo8Dir(currentObject.Position, new Vector3(100, 0, 100)) == safePositions
                               ||
                               PositionTo8Dir(currentObject.Position, new Vector3(100, 0, 100)) == ((safePositions + 4) % 8))
                            {

                                accessory.Method.Mark(accessory.Data.PartyList[temporaryOrder[i]], phase1_markForThePlayersInSafePositions_asAConstant[j]);

                                debugOutput += $"temporaryOrder[i]={temporaryOrder[i]},phase1_markForThePlayersInSafePositions_asAConstant[j]={phase1_markForThePlayersInSafePositions_asAConstant[j]}\n";

                                ++j;

                            }

                        }

                    }

                    if (Enable_Developer_Mode)
                    {

                        accessory.Method.SendChat($"""
                                                   /e 
                                                   {debugOutput}
                                                   
                                                   """);
                        
                        accessory.Log.Debug($"{debugOutput}");

                    }

                });

            }

        }

        [ScriptMethod(name: "Phase1 Clear Marks On Players In Safe Positions",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40158"],
            userControl: false,
            suppress: 2000)]

        public void Phase1_Clear_Marks_On_Players_In_Safe_Positions_æ¸…ç†å®‰å…¨ä½ç½®çŽ©å®¶çš„æ ‡è®°(Event @event, ScriptAccessory accessory)
        {

            if (parse!=1)
            {

                return;

            }

            if (Phase1_Mark_Players_In_Safe_Positions)
            {

                accessory.Method.MarkClear();

            }

        }

        [ScriptMethod(name: "Phase1 Thunder Burnt Strike",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(40164)$"])]

        public void Phase1_Thunder_Burnt_Strike_é›·ç‡ƒçƒ§å‡»(Event @event, ScriptAccessory accessory)
        {

            if (parse!=1)
            {

                return;

            }

            if (!ParseObjectId(@event["SourceId"], out var sourceId))
            {

                return;

            }

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase1_Second_Strike_Of_Thunder_Burnt_Strike";
            currentProperty.Scale = new(20, 40);
            currentProperty.Owner = sourceId;
            currentProperty.Color = Phase1_Colour_Of_Burnt_Strike_Characteristics.V4.WithW(1f);
            currentProperty.Delay = 4000;
            currentProperty.DestoryAt = 5750;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase1_First_Strike_Of_Thunder_Burnt_Strike";
            currentProperty.Scale = new(10, 40);
            currentProperty.Owner = sourceId;
            currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(3f);
            currentProperty.Delay = 4000;
            currentProperty.DestoryAt = 3750;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

        }

        [ScriptMethod(name: "Phase1 Fire Burnt Strike",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(40161)$"])]

        public void Phase1_Fire_Burnt_Strike_ç«ç‡ƒçƒ§å‡»(Event @event, ScriptAccessory accessory)
        {

            if (parse!=1)
            {

                return;

            }

            if (!ParseObjectId(@event["SourceId"], out var sourceId))
            {

                return;

            }

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase1_First_Strike_Of_Fire_Burnt_Strike";
            currentProperty.Scale = new(10, 40);
            currentProperty.Owner = sourceId;
            currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(3f);
            currentProperty.Delay = 4000;
            currentProperty.DestoryAt = 3750;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase1_Central_Axis_Of_Fire_Burnt_Strike";
            currentProperty.Scale = new(0.5f, 40f);
            currentProperty.Owner = sourceId;
            currentProperty.Color = Phase1_Colour_Of_Burnt_Strike_Characteristics.V4.WithW(25f);
            currentProperty.Delay = 4000;
            currentProperty.DestoryAt = 5750;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

            for (int i = 6; i <= 34; i += 7)
            {

                currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase1_Knockback_Direction_Of_Fire_Burnt_Strike";
                currentProperty.Scale = new(1f, 1.618f);
                currentProperty.Owner = sourceId;
                currentProperty.Color = Phase1_Colour_Of_Burnt_Strike_Characteristics.V4.WithW(1f);
                currentProperty.Offset = new Vector3(-5.382f, 0, -i);
                currentProperty.Rotation = float.Pi / 2;
                currentProperty.Delay = 4000;
                currentProperty.DestoryAt = 5750;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, currentProperty);

                currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase1_Knockback_Direction_Of_Fire_Burnt_Strike";
                currentProperty.Scale = new(1f, 1.618f);
                currentProperty.Owner = sourceId;
                currentProperty.Color = Phase1_Colour_Of_Burnt_Strike_Characteristics.V4.WithW(1f);
                currentProperty.Offset = new Vector3(5.382f, 0, -i);
                currentProperty.Rotation = -(float.Pi / 2);
                currentProperty.Delay = 4000;
                currentProperty.DestoryAt = 5750;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, currentProperty);

            }

        }
        
        [ScriptMethod(name: "P1-TurnOfTheHeavens-HaloRange", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4015[23])$"])]
        public void P1_TurnOfTheHeavens_HaloRange(Event evt, ScriptAccessory sa)
        {
            if (!ParseObjectId(@evt["SourceId"], out var sid)) return;
            //var sid = evt.SourceId();
            var delay = 4000;
            var dp = sa.Data.GetDefaultDrawProperties();
            dp.Name = "P1-TurnOfTheHeavens-HaloRange";
            dp.Owner = sid;
            dp.Scale = new(evt["ActionId"] == "40152" ? 5: 10);
            dp.Color = sa.Data.DefaultDangerColor;
            dp.Delay = delay;
            dp.DestoryAt = 8000 - delay;
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "P1_TurnOfTheHeavens_RecordGrabbed", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4165"], userControl: false)]
        public void P1_TurnOfTheHeavens_RecordGrabbed(Event @event, ScriptAccessory accessory)
        {
            if (parse!=1) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            lock (this)
            {
                P1è½¬è½®å¬æŠ“äºº[accessory.Data.PartyList.IndexOf(((uint)tid))] = 1;
            }
        }

        [ScriptMethod(name: "Phase1 Stack Range Of Turn Of The Heavens",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(40152)$"])]

        public void Phase1_Stack_Range_Of_Turn_Of_The_Heavens_å…‰è½®å¬å”¤åˆ†æ‘ŠèŒƒå›´(Event @event, ScriptAccessory accessory)
        {

            if (parse!=1)
            {

                return;

            }

            var currentPosition = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);

            if (MathF.Abs(currentPosition.Z - 100) > 1)
            {

                return;

            }

            bool hasSelectedAStrat = false;
            int highPriorityStack = P1è½¬è½®å¬æŠ“äºº.IndexOf(1);
            int lowPriorityStack = P1è½¬è½®å¬æŠ“äºº.LastIndexOf(1);
            List<int> membersOfTheNorthGroup = [];

            if (Phase1_Group_Of_Turn_Of_The_Heavens == Phase1_Groups_Of_Turn_Of_The_Heavens.MTOTH1H2_Go_North_MTM1_vary_MTSTH1H2åŽ»åŒ—MTD1æ¢_èŽ«çµå–µä¸ŽMMW)
            {

                hasSelectedAStrat = true;

                membersOfTheNorthGroup.Add(highPriorityStack);

                if (1 != highPriorityStack && 1 != lowPriorityStack)
                {

                    membersOfTheNorthGroup.Add(1);

                }

                if (2 != highPriorityStack && 2 != lowPriorityStack)
                {

                    membersOfTheNorthGroup.Add(2);

                }

                if (3 != highPriorityStack && 3 != lowPriorityStack)
                {

                    membersOfTheNorthGroup.Add(3);

                }

                if (membersOfTheNorthGroup.Count < 4
                   &&
                   0 != highPriorityStack
                   &&
                   0 != lowPriorityStack)
                {

                    membersOfTheNorthGroup.Add(0);

                }

                if (membersOfTheNorthGroup.Count < 4
                   &&
                   4 != highPriorityStack
                   &&
                   4 != lowPriorityStack)
                {

                    membersOfTheNorthGroup.Add(4);

                }

            }

            if (Phase1_Group_Of_Turn_Of_The_Heavens == Phase1_Groups_Of_Turn_Of_The_Heavens.MTH1M1R1_Go_North_MTOT_vary_MTH1D1D3åŽ»åŒ—MTSTæ¢)
            {

                hasSelectedAStrat = true;

                membersOfTheNorthGroup.Add(highPriorityStack);

                if (2 != highPriorityStack && 2 != lowPriorityStack)
                {

                    membersOfTheNorthGroup.Add(2);

                }

                if (4 != highPriorityStack && 4 != lowPriorityStack)
                {

                    membersOfTheNorthGroup.Add(4);

                }

                if (6 != highPriorityStack && 6 != lowPriorityStack)
                {

                    membersOfTheNorthGroup.Add(6);

                }

                if (membersOfTheNorthGroup.Count < 4
                   &&
                   0 != highPriorityStack
                   &&
                   0 != lowPriorityStack)
                {

                    membersOfTheNorthGroup.Add(0);

                }

                if (membersOfTheNorthGroup.Count < 4
                   &&
                   1 != highPriorityStack
                   &&
                   1 != lowPriorityStack)
                {

                    membersOfTheNorthGroup.Add(1);

                }

            }

            if (Phase1_Group_Of_Turn_Of_The_Heavens == Phase1_Groups_Of_Turn_Of_The_Heavens.MTOTR1R2_Go_North_MTM1_vary_MTSTD3D4åŽ»åŒ—MTD1æ¢_èŽ«çµå–µ)
            {

                hasSelectedAStrat = true;

                membersOfTheNorthGroup.Add(highPriorityStack);

                if (1 != highPriorityStack && 1 != lowPriorityStack)
                {

                    membersOfTheNorthGroup.Add(1);

                }

                if (6 != highPriorityStack && 6 != lowPriorityStack)
                {

                    membersOfTheNorthGroup.Add(6);

                }

                if (7 != highPriorityStack && 7 != lowPriorityStack)
                {

                    membersOfTheNorthGroup.Add(7);

                }

                if (membersOfTheNorthGroup.Count < 4
                   &&
                   0 != highPriorityStack
                   &&
                   0 != lowPriorityStack)
                {

                    membersOfTheNorthGroup.Add(0);

                }

                if (membersOfTheNorthGroup.Count < 4
                   &&
                   4 != highPriorityStack
                   &&
                   4 != lowPriorityStack)
                {

                    membersOfTheNorthGroup.Add(4);

                }

            }

            if (!hasSelectedAStrat
               ||
               membersOfTheNorthGroup.Count != 4)
            {

                var currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase1_Stack_Range_Of_Turn_Of_The_Heavens";
                currentProperty.Scale = new(6);
                currentProperty.Owner = accessory.Data.PartyList[highPriorityStack];
                currentProperty.Color = accessory.Data.DefaultDangerColor;
                currentProperty.Delay = 6000;
                currentProperty.DestoryAt = 5000;

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);

                currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase1_Stack_Range_Of_Turn_Of_The_Heavens";
                currentProperty.Scale = new(6);
                currentProperty.Owner = accessory.Data.PartyList[lowPriorityStack];
                currentProperty.Color = accessory.Data.DefaultDangerColor;
                currentProperty.Delay = 6000;
                currentProperty.DestoryAt = 5000;

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);

            }

            else
            {

                bool inTheNorthGroup = membersOfTheNorthGroup.Contains(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
                var currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase1_Stack_Range_Of_Turn_Of_The_Heavens";
                currentProperty.Scale = new(6);
                currentProperty.Owner = accessory.Data.PartyList[highPriorityStack];
                currentProperty.Delay = 6000;
                currentProperty.DestoryAt = 5000;

                if (inTheNorthGroup)
                {

                    currentProperty.Color = accessory.Data.DefaultSafeColor;

                }

                else
                {

                    currentProperty.Color = accessory.Data.DefaultDangerColor;

                }

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);

                currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase1_Stack_Range_Of_Turn_Of_The_Heavens";
                currentProperty.Scale = new(6);
                currentProperty.Owner = accessory.Data.PartyList[lowPriorityStack];
                currentProperty.Delay = 6000;
                currentProperty.DestoryAt = 5000;

                if (inTheNorthGroup)
                {

                    currentProperty.Color = accessory.Data.DefaultDangerColor;

                }

                else
                {

                    currentProperty.Color = accessory.Data.DefaultSafeColor;

                }

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);

            }

        }

        [ScriptMethod(name: "P1_TurnOfTheHeavens_KnockbackPosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40152)$"])]
        public void P1_TurnOfTheHeavens_KnockbackPosition(Event @event, ScriptAccessory accessory)
        {
            //dy 7
            if (parse!=1) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            if (MathF.Abs(pos.Z - 100) > 1) return;

            var atEast = pos.X - 100 > 1;
            var o1 = P1è½¬è½®å¬æŠ“äºº.IndexOf(1);
            var o2 = P1è½¬è½®å¬æŠ“äºº.LastIndexOf(1);
            List<int> upGroup = [];
            if (Phase1_Group_Of_Turn_Of_The_Heavens == Phase1_Groups_Of_Turn_Of_The_Heavens.MTOTH1H2_Go_North_MTM1_vary_MTSTH1H2åŽ»åŒ—MTD1æ¢_èŽ«çµå–µä¸ŽMMW)
            {
                upGroup.Add(o1);
                if (o1 != 1 && o2 != 1) upGroup.Add(1);
                if (o1 != 2 && o2 != 2) upGroup.Add(2);
                if (o1 != 3 && o2 != 3) upGroup.Add(3);
                if (upGroup.Count < 4 && o1 != 0 && o2 != 0) upGroup.Add(0);
                if (upGroup.Count < 4 && o1 != 4 && o2 != 4) upGroup.Add(4);
            }
            if (Phase1_Group_Of_Turn_Of_The_Heavens == Phase1_Groups_Of_Turn_Of_The_Heavens.MTH1M1R1_Go_North_MTOT_vary_MTH1D1D3åŽ»åŒ—MTSTæ¢)
            {
                upGroup.Add(o1);
                if (o1 != 2 && o2 != 2) upGroup.Add(2);
                if (o1 != 4 && o2 != 4) upGroup.Add(4);
                if (o1 != 6 && o2 != 6) upGroup.Add(6);
                if (upGroup.Count < 4 && o1 != 0 && o2 != 0) upGroup.Add(0);
                if (upGroup.Count < 4 && o1 != 1 && o2 != 1) upGroup.Add(1);
            }
            if (Phase1_Group_Of_Turn_Of_The_Heavens == Phase1_Groups_Of_Turn_Of_The_Heavens.MTOTR1R2_Go_North_MTM1_vary_MTSTD3D4åŽ»åŒ—MTD1æ¢_èŽ«çµå–µ)
            {
                /* (commented out as in original)
                List<int> upIndex = [0, 1, 6, 7];
                if (upIndex.Contains(o1) && !upIndex.Contains(o2)) upGroup.Add(o1);
                if (upIndex.Contains(o2) && !upIndex.Contains(o1)) upGroup.Add(o2);
                if (upIndex.Contains(o1) && !upIndex.Contains(o2))
                {
                    if (upIndex.IndexOf(o1)<upIndex.IndexOf(o2))
                    {
                        upGroup.Add(o1);
                    }
                    else
                    {
                        upGroup.Add(o2);
                    }
                }
                var up0 = upGroup[0];
                var down0 = up0 == o1 ? o2 : o1;
                if (up0 != 1 && down0 != 1) upGroup.Add(1);
                if (up0 != 6 && down0 != 6) upGroup.Add(6);
                if (up0 != 7 && down0 != 7) upGroup.Add(7);
                if (upGroup.Count < 4 && up0 != 0 && down0 != 0) upGroup.Add(0);
                if (upGroup.Count < 4 && up0 != 4 && down0 != 4) upGroup.Add(4);
                */

                // After carefully examining the above algorithm, Cicero may consider that it's wrong.
                // Even if it's correct, it may contain a bunch of redundant steps.
                // Therefore, Cicero just simply decided to comment it out and rework a new algorithm similar to that of other groups.

                upGroup.Add(o1);
                if (o1 != 1 && o2 != 1) upGroup.Add(1);
                if (o1 != 6 && o2 != 6) upGroup.Add(6);
                if (o1 != 7 && o2 != 7) upGroup.Add(7);
                if (upGroup.Count < 4 && o1 != 0 && o2 != 0) upGroup.Add(0);
                if (upGroup.Count < 4 && o1 != 4 && o2 != 4) upGroup.Add(4);
            }

            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var dealpos1 = new Vector3(atEast ? 105.5f : 94.5f, 0, upGroup.Contains(myindex) ? 93 : 107);
            var dealpos2 = new Vector3(atEast ? 102 : 98, 0, upGroup.Contains(myindex) ? 93 : 107);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_TurnOfTheHeavens_KnockbackPosition1";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos1;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_TurnOfTheHeavens_KnockbackPosition2";
            dp.Scale = new(2);
            dp.Position = dealpos1;
            dp.TargetPosition = dealpos2;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P1_TurnOfTheHeavens_KnockbackPosition3";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos2;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 4000;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);


        }

        [ScriptMethod(name: "Phase1 Fall Of Faith Control",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(40170)$"],
            userControl: false)]

        public void Phase1_Fall_Of_Faith_Control_ä¿¡ä»°å´©å¡ŒæŽ§åˆ¶(Event @event, ScriptAccessory accessory)
        {

            if (parse!=1)
            {

                return;

            }

            System.Threading.Thread.MemoryBarrier();

            ++phase1_timesBurnishedGloryWasCast;

            System.Threading.Thread.MemoryBarrier();

            if (Enable_Developer_Mode)
            {

                accessory.Method.SendChat($"""
                                           /e 
                                           phase1_timesBurnishedGloryWasCast={phase1_timesBurnishedGloryWasCast}
                                           
                                           """);

            }

            switch (phase1_timesBurnishedGloryWasCast)
            {

                case 1:
                    {

                        phase1_tetheredPlayersDuringFallOfFaith.Clear();

                        if (Phase1_Mark_Players_During_Fall_Of_Faith)
                        {

                            accessory.Method.MarkClear();

                        }

                        phase1_semaphoreOfMarkingTetheredPlayers = 0;
                        phase1_semaphoreOfShortPrompts = 0;
                        phase1_semaphoreOfDrawing = 0;
                        phase1_semaphoreOfMarkingUntetheredPlayers = 0;
                        phase1_semaphoreOfTheFinalPrompt = 0;

                        System.Threading.Thread.MemoryBarrier();

                        phase1_isInFallOfFaith = true;

                        break;

                    }

                case 2:
                    {

                        phase1_isInFallOfFaith = false;

                        System.Threading.Thread.MemoryBarrier();

                        phase1_tetheredPlayersDuringFallOfFaith.Clear();

                        if (Phase1_Mark_Players_During_Fall_Of_Faith)
                        {

                            accessory.Method.MarkClear();

                        }

                        phase1_semaphoreOfMarkingTetheredPlayers = 0;
                        phase1_semaphoreOfShortPrompts = 0;
                        phase1_semaphoreOfDrawing = 0;
                        phase1_semaphoreOfMarkingUntetheredPlayers = 0;
                        phase1_semaphoreOfTheFinalPrompt = 0;

                        break;

                    }

                default:
                    {

                        phase1_tetheredPlayersDuringFallOfFaith.Clear();

                        if (Phase1_Mark_Players_During_Fall_Of_Faith)
                        {

                            accessory.Method.MarkClear();

                        }

                        phase1_semaphoreOfMarkingTetheredPlayers = 0;
                        phase1_semaphoreOfShortPrompts = 0;
                        phase1_semaphoreOfDrawing = 0;
                        phase1_semaphoreOfMarkingUntetheredPlayers = 0;
                        phase1_semaphoreOfTheFinalPrompt = 0;

                        break;
                        // Just a placeholder and should never be reached.

                    }

            }

        }

        [ScriptMethod(name: "Phase1 Record Tethered Players",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:regex:^(00F9|011F)$"],
            userControl: false)]

        public void Phase1_Record_Tethered_Players_è®°å½•è¢«è¿žçº¿çš„çŽ©å®¶(Event @event, ScriptAccessory accessory)
        {

            if (parse!=1)
            {

                return;

            }

            if (!phase1_isInFallOfFaith)
            {

                return;

            }

            if (!ParseObjectId(@event["TargetId"], out var targetId))
            {

                return;

            }

            int targetIndex = accessory.Data.PartyList.IndexOf(((uint)targetId));
            var tetherType = (@event["Id"].Equals("00F9")) ? (10) : (20);
            // 10 stands for a fire tether.

            System.Threading.Thread.MemoryBarrier();

            phase1_tetheredPlayersDuringFallOfFaith.Add(tetherType + targetIndex);

            System.Threading.Thread.MemoryBarrier();

            phase1_semaphoreOfMarkingTetheredPlayers = 1;
            phase1_semaphoreOfShortPrompts = 1;
            phase1_semaphoreOfDrawing = 1;
            phase1_semaphoreOfMarkingUntetheredPlayers = 1;
            phase1_semaphoreOfTheFinalPrompt = 1;

        }

        [ScriptMethod(name: "Phase1 Mark Tethered Players",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:regex:^(00F9|011F)$"],
            userControl: false)]

        public void Phase1_Mark_Tethered_Players_æ ‡è®°è¢«è¿žçº¿çš„çŽ©å®¶(Event @event, ScriptAccessory accessory)
        {

            if (!Phase1_Mark_Players_During_Fall_Of_Faith)
            {

                return;

            }

            if (parse!=1)
            {

                return;

            }

            if (!phase1_isInFallOfFaith)
            {

                return;

            }

            while (System.Threading.Interlocked.CompareExchange(ref phase1_semaphoreOfMarkingTetheredPlayers, 0, 1) == 0)
            {

                System.Threading.Thread.Sleep(1);

            }

            System.Threading.Thread.MemoryBarrier();

            int copyOfTheCount = phase1_tetheredPlayersDuringFallOfFaith.Count;
            int targetIndex = (phase1_tetheredPlayersDuringFallOfFaith.Last() % 10);
            MarkType targetMark = phase1_markForTheTetheredPlayer_asAConstant[copyOfTheCount - 1];

            accessory.Method.Mark(accessory.Data.PartyList[targetIndex], targetMark);

            if (Enable_Developer_Mode)
            {

                accessory.Method.SendChat($"""
                                           /e 
                                           copyOfTheCount-1={copyOfTheCount - 1}
                                           targetIndex={targetIndex}
                                           targetMark={targetMark}
                                           
                                           """);

            }

        }

        [ScriptMethod(name: "Phase1 Prompt The Type Of The Current Tether",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:regex:^(00F9|011F)$"])]

        public void Phase1_Prompt_The_Type_Of_The_Current_Tether_æç¤ºå½“å‰è¿žçº¿çš„ç±»åž‹(Event @event, ScriptAccessory accessory)
        {

            if (parse!=1)
            {

                return;

            }

            if (!phase1_isInFallOfFaith)
            {

                return;

            }

            while (System.Threading.Interlocked.CompareExchange(ref phase1_semaphoreOfShortPrompts, 0, 1) == 0)
            {

                System.Threading.Thread.Sleep(1);

            }

            System.Threading.Thread.MemoryBarrier();

            if (1 <= phase1_tetheredPlayersDuringFallOfFaith.Count && phase1_tetheredPlayersDuringFallOfFaith.Count <= 3)
            {

                bool isFireTether = (phase1_tetheredPlayersDuringFallOfFaith.Last() < 20);
                string prompt = "";

                if (isFireTether)
                {

                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                    {

                        prompt = "ç«";

                    }

                    if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                    {

                        prompt = "Fire";

                    }

                }

                else
                {

                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                    {

                        prompt = "é›·";

                    }

                    if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                    {

                        prompt = "Thunder";

                    }

                }

                if (!prompt.Equals(""))
                {

                    if (Enable_Text_Prompts)
                    {

                        accessory.Method.TextInfo(prompt, 1000);

                    }

                    if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
                    {

                        accessory.TTS(prompt, Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                    }

                }

            }

        }

        [ScriptMethod(name: "Phase1 Range Of The Current Tether",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:regex:^(00F9|011F)$"])]

        public void Phase1_Range_Of_The_Current_Tether_å½“å‰è¿žçº¿çš„èŒƒå›´(Event @event, ScriptAccessory accessory)
        {

            if (parse!=1)
            {

                return;

            }

            if (!phase1_isInFallOfFaith)
            {

                return;

            }

            while (System.Threading.Interlocked.CompareExchange(ref phase1_semaphoreOfDrawing, 0, 1) == 0)
            {

                System.Threading.Thread.Sleep(1);

            }

            System.Threading.Thread.MemoryBarrier();

            bool isFireTether = (phase1_tetheredPlayersDuringFallOfFaith.Last() < 20);
            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            if (isFireTether)
            {

                currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase1_Range_Of_The_Fire_Tether";
                currentProperty.Scale = new(60);
                currentProperty.Radian = float.Pi / 2;
                currentProperty.Owner = accessory.Data.PartyList[(phase1_tetheredPlayersDuringFallOfFaith.Last() % 10)];
                currentProperty.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                currentProperty.TargetOrderIndex = 1;
                currentProperty.Color = accessory.Data.DefaultDangerColor;
                currentProperty.Delay = 9500;
                currentProperty.DestoryAt = 3800;

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, currentProperty);

            }

            else
            {

                for (uint i = 1; i <= 3; ++i)
                {

                    currentProperty = accessory.Data.GetDefaultDrawProperties();

                    currentProperty.Name = "Phase1_Range_Of_The_Thunder_Tether";
                    currentProperty.Scale = new(60);
                    currentProperty.Radian = float.Pi / 3 * 2;
                    currentProperty.Owner = accessory.Data.PartyList[(phase1_tetheredPlayersDuringFallOfFaith.Last() % 10)];
                    currentProperty.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                    currentProperty.TargetOrderIndex = i;
                    currentProperty.Color = accessory.Data.DefaultDangerColor;
                    currentProperty.Delay = 9500;
                    currentProperty.DestoryAt = 3800;

                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, currentProperty);

                }

            }

        }

        [ScriptMethod(name: "Phase1 Mark Untethered Players",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:regex:^(00F9|011F)$"],
            userControl: false)]

        public void Phase1_Mark_Untethered_Players_æ ‡è®°æœªè¢«è¿žçº¿çš„çŽ©å®¶(Event @event, ScriptAccessory accessory)
        {

            if (!Phase1_Mark_Players_During_Fall_Of_Faith)
            {

                return;

            }

            if (parse!=1)
            {

                return;

            }

            if (!phase1_isInFallOfFaith)
            {

                return;

            }

            while (System.Threading.Interlocked.CompareExchange(ref phase1_semaphoreOfMarkingUntetheredPlayers, 0, 1) == 0)
            {

                System.Threading.Thread.Sleep(1);

            }

            System.Threading.Thread.MemoryBarrier();

            if (phase1_tetheredPlayersDuringFallOfFaith.Count != 4)
            {

                return;

            }

            var tetheredPlayers = phase1_tetheredPlayersDuringFallOfFaith.Select(o => o % 10).ToList();
            List<int> untetheredPlayers = [];

            if (Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Single_Line_In_THD_Order_æŒ‰THDé¡ºåºå•æŽ’
               ||
               Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Double_Lines_MOTH12_Left_M12R12_Right_åŒæŽ’å·¦MSTH12å³D1234)
            {

                for (int i = 0; i < accessory.Data.PartyList.Count; ++i)
                {

                    if (!tetheredPlayers.Contains(i))
                    {

                        untetheredPlayers.Add(i);

                    }

                }

            }

            if (Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Single_Line_In_HTD_Order_æŒ‰HTDé¡ºåºå•æŽ’_èŽ«çµå–µ
               ||
               Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Double_Lines_H12MOT_Left_M12R12_Right_åŒæŽ’å·¦H12MSTå³D1234_èŽ«çµå–µä¸ŽMMW)
            {
                // The addition of this strat credits to @alexandria_prime. Appreciate!

                List<int> temporaryPriority = new List<int> { 2, 3, 0, 1, 4, 5, 6, 7 };

                for (int i = 0; i < temporaryPriority.Count; ++i)
                {

                    if (!tetheredPlayers.Contains(temporaryPriority[i]))
                    {

                        untetheredPlayers.Add(temporaryPriority[i]);

                    }

                }

            }

            if (Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Single_Line_In_H1TDH2_Order_æŒ‰H1TDH2é¡ºåºå•æŽ’)
            {
                // The addition of this strat credits to @alexandria_prime. Appreciate!

                List<int> temporaryPriority = new List<int> { 2, 0, 1, 4, 5, 6, 7, 3 };

                for (int i = 0; i < temporaryPriority.Count; ++i)
                {

                    if (!tetheredPlayers.Contains(temporaryPriority[i]))
                    {

                        untetheredPlayers.Add(temporaryPriority[i]);

                    }

                }

            }

            if (untetheredPlayers.Count != 4)
            {

                return;

            }

            string debugOutput = "";

            for (int i = 0; i < untetheredPlayers.Count; ++i)
            {

                accessory.Method.Mark(accessory.Data.PartyList[(untetheredPlayers[i])], phase1_markForTheUntetheredPlayer_asAConstant[i]);

                if (Enable_Developer_Mode)
                {

                    debugOutput += $"(untetheredPlayers[{i}])={(untetheredPlayers[i])}\n";
                    debugOutput += $"phase1_markForTheUntetheredPlayer_asAConstant[i]={phase1_markForTheUntetheredPlayer_asAConstant[i]}\n";

                }

            }

            if (Enable_Developer_Mode)
            {

                accessory.Method.SendChat($"""
                                           /e 
                                           {debugOutput}

                                           """);
                
                accessory.Log.Debug($"{debugOutput}");

            }

        }

        [ScriptMethod(name: "Phase1 Prompt All The Types Of Tethers",
            eventType: EventTypeEnum.Tether,
            eventCondition: ["Id:regex:^(00F9|011F)$"])]

        public void Phase1_Prompt_All_The_Types_Of_Tethers_æç¤ºæ‰€æœ‰è¿žçº¿çš„ç±»åž‹(Event @event, ScriptAccessory accessory)
        {

            if (parse!=1)
            {

                return;

            }

            if (!phase1_isInFallOfFaith)
            {

                return;

            }

            while (System.Threading.Interlocked.CompareExchange(ref phase1_semaphoreOfTheFinalPrompt, 0, 1) == 0)
            {

                System.Threading.Thread.Sleep(1);

            }

            System.Threading.Thread.MemoryBarrier();

            if (phase1_tetheredPlayersDuringFallOfFaith.Count != 4)
            {

                return;

            }

            var isFireTether = phase1_tetheredPlayersDuringFallOfFaith.Select(o => o < 20).ToList();

            if (isFireTether.Count != 4)
            {

                return;

            }

            string prompt = "";

            if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
            {

                prompt += (isFireTether[0]) ? "ç«" : "é›·";

            }

            if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
            {

                prompt += (isFireTether[0]) ? "Fire" : "Thunder";

            }

            for (int i = 1; i < isFireTether.Count; ++i)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    prompt += (isFireTether[i]) ? ",ç«" : ",é›·";

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    prompt += (isFireTether[i]) ? ", Fire" : ", Thunder";

                }

            }

            if (!prompt.Equals(""))
            {

                if (Enable_Text_Prompts)
                {

                    accessory.Method.TextInfo(prompt, 13300);

                }

                if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
                {

                    accessory.TTS(prompt, Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

            }

        }

        [ScriptMethod(name: "P1_FallOfFaith_Position", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(00F9|011F)$"])]
        public void P1_FallOfFaith_Position(Event @event, ScriptAccessory accessory)
        {
            if (!phase1_isInFallOfFaith) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var dis = 2.5f;//distance from named player
            var far = 5.25f;//distance from boss
            Task.Delay(334).ContinueWith(t =>
            {
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                Vector3 t1p1 = new(100, 0, 100 - far);
                Vector3 t1p2 = new(100, 0, 100 - far - dis);
                Vector3 t2p1 = new(100, 0, 100 + far);
                Vector3 t2p2 = new(100, 0, 100 + far + dis);
                Vector3 t3p1 = new(100, 0, 100 - far - dis);
                Vector3 t3p2 = new(100, 0, 100 - far);
                Vector3 t4p1 = new(100, 0, 100 + far + dis);
                Vector3 t4p2 = new(100, 0, 100 + far);

                if (phase1_tetheredPlayersDuringFallOfFaith.Count == 1 && tid == accessory.Data.Me)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FallOfFaith_Position1_1";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t1p1;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 13000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FallOfFaith_Position1_2";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t1p2;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 13000;
                    dp.DestoryAt = 6000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    if (t1p1 != t1p2)
                    {
                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_FallOfFaith_Position1_2Preview";
                        dp.Scale = new(2);
                        dp.Position = t1p1;
                        dp.TargetPosition = t1p2;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultDangerColor;
                        dp.DestoryAt = 13000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                }
                if (phase1_tetheredPlayersDuringFallOfFaith.Count == 2 && tid == accessory.Data.Me)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FallOfFaith_Position2_1";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t2p1;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 13500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FallOfFaith_Position2_2";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t2p2;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 13500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    if (t2p1 != t2p2)
                    {
                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_FallOfFaith_Position2_2Preview";
                        dp.Scale = new(2);
                        dp.Position = t2p1;
                        dp.TargetPosition = t2p2;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultDangerColor;
                        dp.DestoryAt = 13500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                }
                if (phase1_tetheredPlayersDuringFallOfFaith.Count == 3 && tid == accessory.Data.Me)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FallOfFaith_Position3_1";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t3p1;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 7500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FallOfFaith_Position3_2";
                    dp.Scale = new(3);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t3p2;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 7500;
                    dp.DestoryAt = 6000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    if (t3p1 != t3p2)
                    {
                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_FallOfFaith_Position3_2Preview";
                        dp.Scale = new(2);
                        dp.Position = t3p1;
                        dp.TargetPosition = t3p2;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultDangerColor;
                        dp.DestoryAt = 7500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                }
                if (phase1_tetheredPlayersDuringFallOfFaith.Count == 4 && tid == accessory.Data.Me)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FallOfFaith_Position4_1";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t4p1;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 8500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FallOfFaith_Position4_2";
                    dp.Scale = new(3);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = t4p2;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 8500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    if (t4p1 != t4p2)
                    {
                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_FallOfFaith_Position4_2Preview";
                        dp.Scale = new(2);
                        dp.Position = t4p1;
                        dp.TargetPosition = t4p2;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultDangerColor;
                        dp.DestoryAt = 8500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                }
                if (phase1_tetheredPlayersDuringFallOfFaith.Count == 4)
                {
                    var tehterObjIndex = phase1_tetheredPlayersDuringFallOfFaith.Select(o => o % 10).ToList();
                    var tehterIsFire = phase1_tetheredPlayersDuringFallOfFaith.Select(o => o < 20).ToList();
                    List<int> idleObjIndex = [];
                    if (Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Single_Line_In_THD_Order_æŒ‰THDé¡ºåºå•æŽ’
                        ||
                        Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Double_Lines_MOTH12_Left_M12R12_Right_åŒæŽ’å·¦MSTH12å³D1234)
                    {
                        for (int i = 0; i < accessory.Data.PartyList.Count; i++)
                        {
                            if (!tehterObjIndex.Contains(i))
                            { idleObjIndex.Add(i); }
                        }
                    }

                    if (Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Single_Line_In_HTD_Order_æŒ‰HTDé¡ºåºå•æŽ’_èŽ«çµå–µ
                       ||
                       Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Double_Lines_H12MOT_Left_M12R12_Right_åŒæŽ’å·¦H12MSTå³D1234_èŽ«çµå–µä¸ŽMMW)
                    {
                        // The addition of this strat credits to @alexandria_prime. Appreciate!

                        List<int> htdOrder = new List<int> { 2, 3, 0, 1, 4, 5, 6, 7 };

                        for (int i = 0; i < htdOrder.Count; ++i)
                        {

                            if (!tehterObjIndex.Contains(htdOrder[i]))
                            {

                                idleObjIndex.Add(htdOrder[i]);

                            }

                        }

                    }

                    if (Phase1_Strat_Of_Fall_Of_Faith == Phase1_Strats_Of_Fall_Of_Faith.Single_Line_In_H1TDH2_Order_æŒ‰H1TDH2é¡ºåºå•æŽ’)
                    {
                        // The addition of this strat credits to @alexandria_prime. Appreciate!

                        List<int> h1tdh2Order = new List<int> { 2, 0, 1, 4, 5, 6, 7, 3 };

                        for (int i = 0; i < h1tdh2Order.Count; ++i)
                        {

                            if (!tehterObjIndex.Contains(h1tdh2Order[i]))
                            {

                                idleObjIndex.Add(h1tdh2Order[i]);

                            }

                        }

                    }

                    if (!idleObjIndex.Contains(myindex)) return;

                    Vector3 i1p1 = new Vector3(100, 0, 100);
                    Vector3 i1p2 = new Vector3(100, 0, 100);
                    Vector3 i2p1 = new Vector3(100, 0, 100);
                    Vector3 i2p2 = new Vector3(100, 0, 100);
                    Vector3 i3p1 = new Vector3(100, 0, 100);
                    Vector3 i3p2 = new Vector3(100, 0, 100);
                    Vector3 i4p1 = new Vector3(100, 0, 100);
                    Vector3 i4p2 = new Vector3(100, 0, 100);
                    Vector3 dealpos1 = default;
                    Vector3 dealpos2 = default;

                    if (Phase1_Orientation_Benchmark_During_Fall_Of_Faith == Phase1_Orientation_Benchmarks_During_Fall_Of_Faith.High_Priority_Left_Facing_Due_North_é¢å‘æ­£åŒ—å·¦ä¾§é«˜ä¼˜å…ˆçº§)
                    {

                        i1p1 = tehterIsFire[0] ? new(100, 0, 100 - far - dis) : new(100 - dis, 0, 100 - far);
                        i1p2 = tehterIsFire[2] ? new(100, 0, 100 - far - dis) : new(100 - dis, 0, 100 - far);
                        i2p1 = tehterIsFire[0] ? new(100, 0, 100 - far - dis) : new(100 + dis, 0, 100 - far);
                        i2p2 = tehterIsFire[2] ? new(100, 0, 100 - far - dis) : new(100 + dis, 0, 100 - far);
                        i3p1 = tehterIsFire[1] ? new(100, 0, 100 + far + dis) : new(100 - dis, 0, 100 + far);
                        i3p2 = tehterIsFire[3] ? new(100, 0, 100 + far + dis) : new(100 - dis, 0, 100 + far);
                        i4p1 = tehterIsFire[1] ? new(100, 0, 100 + far + dis) : new(100 + dis, 0, 100 + far);
                        i4p2 = tehterIsFire[3] ? new(100, 0, 100 + far + dis) : new(100 + dis, 0, 100 + far);

                    }

                    if (Phase1_Orientation_Benchmark_During_Fall_Of_Faith == Phase1_Orientation_Benchmarks_During_Fall_Of_Faith.High_Priority_Left_Facing_The_Boss_é¢å‘Bosså·¦ä¾§é«˜ä¼˜å…ˆçº§_èŽ«çµå–µä¸ŽMMW)
                    {
                        // The addition of this benchmark credits to @alexandria_prime. Appreciate!

                        i1p1 = tehterIsFire[0] ? new(100, 0, 100 - far - dis) : new(100 + dis, 0, 100 - far);
                        i1p2 = tehterIsFire[2] ? new(100, 0, 100 - far - dis) : new(100 + dis, 0, 100 - far);
                        i2p1 = tehterIsFire[0] ? new(100, 0, 100 - far - dis) : new(100 - dis, 0, 100 - far);
                        i2p2 = tehterIsFire[2] ? new(100, 0, 100 - far - dis) : new(100 - dis, 0, 100 - far);
                        i3p1 = tehterIsFire[1] ? new(100, 0, 100 + far + dis) : new(100 - dis, 0, 100 + far);
                        i3p2 = tehterIsFire[3] ? new(100, 0, 100 + far + dis) : new(100 - dis, 0, 100 + far);
                        i4p1 = tehterIsFire[1] ? new(100, 0, 100 + far + dis) : new(100 + dis, 0, 100 + far);
                        i4p2 = tehterIsFire[3] ? new(100, 0, 100 + far + dis) : new(100 + dis, 0, 100 + far);

                    }

                    if (i1p1.Equals(new Vector3(100, 0, 100)))
                    {

                        return;

                    }

                    dealpos1 = idleObjIndex.IndexOf(myindex) switch
                    {
                        0 => i1p1,
                        1 => i2p1,
                        2 => i3p1,
                        3 => i4p1,
                    };
                    dealpos2 = idleObjIndex.IndexOf(myindex) switch
                    {
                        0 => i1p2,
                        1 => i2p2,
                        2 => i3p2,
                        3 => i4p2,
                    };
                    var upgroup = (idleObjIndex.IndexOf(myindex) == 0 || idleObjIndex.IndexOf(myindex) == 1);

                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FallOfFaith_PositionIdle1";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = dealpos1;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = upgroup ? 5000 : 8500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P1_FallOfFaith_PositionIdle2";
                    dp.Scale = new(2);
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = dealpos2;
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = upgroup ? 5000 : 8500;
                    dp.DestoryAt = upgroup ? 6000 : 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    if (dealpos1 != dealpos2)
                    {
                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_FallOfFaith_PositionIdle2Preview";
                        dp.Scale = new(2);
                        dp.Position = dealpos1;
                        dp.TargetPosition = dealpos2;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultDangerColor;
                        dp.DestoryAt = upgroup ? 5000 : 8500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                }
            });
        }

        [ScriptMethod(name: "P1_Towers_Recorder", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4012[234567]|4013[15])$"], userControl: false)]
        public void P1_Towers_Recorder(Event @event, ScriptAccessory accessory)
        {
            if (parse!=1) return;
            lock (this)
            {
                var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                var count = @event["ActionId"] switch
                {
                    "40135" => 1,
                    "40131" => 1,
                    "40122" => 2,
                    "40123" => 3,
                    "40124" => 4,
                    "40125" => 2,
                    "40126" => 3,
                    "40127" => 4,
                };
                if (MathF.Abs(pos.Z - 100) < 1)
                {
                    P1å¡”[1] = count;
                }
                else
                {
                    if (pos.Z - 100 > 1) P1å¡”[2] = count;
                    else P1å¡”[0] = count;
                }
                if (pos.X - 100 > 1)
                {
                    P1å¡”[3] = 1;
                }
            }
        }

        [ScriptMethod(name: "Phase1 Burnt Strike With Towers And Tank Busters",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:regex:^(40134|40129)$"])]

        public void Phase1_Burnt_Strike_With_Towers_And_Tank_Busters_å¸¦æœ‰å¡”å’Œæ­»åˆ‘çš„ç‡ƒçƒ§å‡»(Event @event, ScriptAccessory accessory)
        {

            if (parse!=1)
            {

                return;

            }

            if (!ParseObjectId(@event["SourceId"], out var sourceId))
            {

                return;

            }

            if (@event["ActionId"].Equals("40134"))
            {
                // Thunder Burnt Strike.

                var currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase1_Second_Strike_Of_Thunder_Burnt_Strike";
                currentProperty.Scale = new(20, 40);
                currentProperty.Owner = sourceId;
                currentProperty.Color = Phase1_Colour_Of_Burnt_Strike_Characteristics.V4.WithW(1f);
                currentProperty.DestoryAt = 8200;

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, currentProperty);

                currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase1_First_Strike_Of_Thunder_Burnt_Strike";
                currentProperty.Scale = new(10, 40);
                currentProperty.Owner = sourceId;
                currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(3f);
                currentProperty.DestoryAt = 6500;

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, currentProperty);

            }

            if (@event["ActionId"].Equals("40129"))
            {
                // Fire Burnt Strike.

                var currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase1_First_Strike_Of_Fire_Burnt_Strike";
                currentProperty.Scale = new(10, 40);
                currentProperty.Owner = sourceId;
                currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(3f);
                currentProperty.DestoryAt = 6500;

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, currentProperty);

                currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase1_Central_Axis_Of_Fire_Burnt_Strike";
                currentProperty.Scale = new(0.5f, 40f);
                currentProperty.Owner = sourceId;
                currentProperty.Color = Phase1_Colour_Of_Burnt_Strike_Characteristics.V4.WithW(25f);
                currentProperty.DestoryAt = 8200;

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Straight, currentProperty);

                for (int i = -4; i <= 4; ++i)
                {

                    currentProperty = accessory.Data.GetDefaultDrawProperties();

                    currentProperty.Name = "Phase1_Knockback_Direction_Of_Fire_Burnt_Strike";
                    currentProperty.Scale = new(1f, 1.618f);
                    currentProperty.Owner = sourceId;
                    currentProperty.Color = Phase1_Colour_Of_Burnt_Strike_Characteristics.V4.WithW(1f);
                    currentProperty.Offset = new Vector3(-5.382f, 0, (float)(-(i * 4.595d)));
                    currentProperty.Rotation = float.Pi / 2;
                    currentProperty.DestoryAt = 8200;

                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, currentProperty);

                    currentProperty = accessory.Data.GetDefaultDrawProperties();

                    currentProperty.Name = "Phase1_Knockback_Direction_Of_Fire_Burnt_Strike";
                    currentProperty.Scale = new(1f, 1.618f);
                    currentProperty.Owner = sourceId;
                    currentProperty.Color = Phase1_Colour_Of_Burnt_Strike_Characteristics.V4.WithW(1f);
                    currentProperty.Offset = new Vector3(5.382f, 0, (float)(-(i * 4.595d)));
                    currentProperty.Rotation = -(float.Pi / 2);
                    currentProperty.DestoryAt = 8200;

                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Arrow, currentProperty);

                }

            }

        }

        [ScriptMethod(name: "P1_Towers_Position", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40134|40129)$"])]
        public void P1_Towers_Position(Event @event, ScriptAccessory accessory)
        {
            if (parse!=1) return;
            Task.Delay(334).ContinueWith(t =>
            {
                var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                if (@event["ActionId"] == "40134")
                {
                    var eastTower = P1å¡”[3] == 1;
                    //Thunder
                    if (myIndex == 0 || myIndex == 1)
                    {
                        var dx = eastTower ? -10.5f : 10.5f;
                        var dy = myIndex == 0 ? -5.5f : 5.5f;
                        // The expression was myindex == 1 ? -5.5f : 5.5f before. Obviously it reverses the situation of MT and OT.
                        // The bug fix here credits to @alexandria_prime. Appreciate!
                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Towers_Position_Thunder_T";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = new(100 + dx, 0, 100 + dy);
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 10500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                    else
                    {
                        int myTowerIndex = myIndex - 1;
                        Vector3 standbyPosition = new Vector3(100, 0, 100);
                        Vector3 towerPosition = new Vector3(100, 0, 100);

                        if (Phase1_Strat_Of_Towers == Phase1_Strats_Of_Towers.Completely_Based_On_Priority_å®Œå…¨æ ¹æ®ä¼˜å…ˆçº§_èŽ«çµå–µ)
                        {

                            if (myTowerIndex > 0 && myTowerIndex <= P1å¡”[0]) standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);
                            if (myTowerIndex > P1å¡”[0] && myTowerIndex <= P1å¡”[0] + P1å¡”[1]) standbyPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);
                            if (myTowerIndex > P1å¡”[0] + P1å¡”[1] && myTowerIndex <= P1å¡”[0] + P1å¡”[1] + P1å¡”[2]) standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);

                            if (myTowerIndex > 0 && myTowerIndex <= P1å¡”[0]) towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);
                            if (myTowerIndex > P1å¡”[0] && myTowerIndex <= P1å¡”[0] + P1å¡”[1]) towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);
                            if (myTowerIndex > P1å¡”[0] + P1å¡”[1] && myTowerIndex <= P1å¡”[0] + P1å¡”[1] + P1å¡”[2]) towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);

                        }

                        if (Phase1_Strat_Of_Towers == Phase1_Strats_Of_Towers.Fixed_H1H2R2_Priority_For_Rest_å›ºå®šH1H2D4å‰©ä½™äººä¼˜å…ˆçº§)
                        {

                            bool fixedPartyMember = false;

                            if (myIndex == 2)
                            {

                                fixedPartyMember = true;

                                standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);
                                towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);

                            }

                            if (myIndex == 3)
                            {

                                fixedPartyMember = true;

                                standbyPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);
                                towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);

                            }

                            if (myIndex == 7)
                            {

                                fixedPartyMember = true;

                                standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);
                                towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);

                            }

                            if (!fixedPartyMember)
                            {

                                int newTower0 = P1å¡”[0] - 1;
                                int newTower1 = P1å¡”[1] - 1;
                                int newTower2 = P1å¡”[2] - 1;
                                int myNewTowerIndex = myIndex - 3;

                                if (Enable_Developer_Mode)
                                {

                                    accessory.Method.SendChat($"""
                                                               /e 
                                                               newTower0={newTower0}
                                                               newTower1={newTower1}
                                                               newTower2={newTower2}
                                                               myNewTowerIndex={myNewTowerIndex}
                                                               
                                                               """);

                                }

                                if (newTower0 > 0
                                    &&
                                    0 < myNewTowerIndex && myNewTowerIndex <= newTower0)
                                {

                                    standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);
                                    towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);

                                }

                                if (newTower1 > 0
                                   &&
                                   newTower0 < myNewTowerIndex && myNewTowerIndex <= newTower0 + newTower1)
                                {

                                    standbyPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);
                                    towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);

                                }

                                if (newTower2 > 0
                                   &&
                                   newTower0 + newTower1 < myNewTowerIndex && myNewTowerIndex <= newTower0 + newTower1 + newTower2)
                                {

                                    standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);
                                    towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);

                                }

                            }

                        }

                        if (Phase1_Strat_Of_Towers == Phase1_Strats_Of_Towers.Fixed_H1H2R2_Rest_Fill_Vacancies_å›ºå®šH1H2D4å‰©ä½™äººè¡¥ä½_MMW)
                        {
                            // The algorithm implementation of this strat was inspired by @abigseal's script.
                            // Therefore, the following code should credit to him. Appreciate!

                            bool fixedPartyMember = false;

                            if (myIndex == 2)
                            {

                                fixedPartyMember = true;

                                standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);
                                towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);

                            }

                            if (myIndex == 3)
                            {

                                fixedPartyMember = true;

                                standbyPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);
                                towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);

                            }

                            if (myIndex == 7)
                            {

                                fixedPartyMember = true;

                                standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);
                                towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);

                            }

                            if (!fixedPartyMember)
                            {

                                if (Enable_Developer_Mode)
                                {

                                    accessory.Method.SendChat($"""
                                                               /e 
                                                               P1å¡”[0]={P1å¡”[0]}
                                                               P1å¡”[1]={P1å¡”[1]}
                                                               P1å¡”[2]={P1å¡”[2]}
                                                               myTowerIndex={myTowerIndex}

                                                               """);

                                }

                                if (myIndex == 4)
                                {

                                    if (P1å¡”[0] >= 2)
                                    {

                                        standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);
                                        towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);

                                    }

                                    else
                                    {

                                        if (P1å¡”[1] >= 3)
                                        {

                                            standbyPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);
                                            towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);

                                        }

                                        if (P1å¡”[2] >= 3)
                                        {

                                            standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);
                                            towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);

                                        }

                                    }

                                }

                                if (myIndex == 5)
                                {

                                    if (P1å¡”[1] >= 2)
                                    {

                                        standbyPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);
                                        towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);

                                    }

                                    else
                                    {

                                        if (P1å¡”[0] >= 3)
                                        {

                                            standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);
                                            towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);

                                        }

                                        if (P1å¡”[2] >= 3)
                                        {

                                            standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);
                                            towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);

                                        }

                                    }

                                }

                                if (myIndex == 6)
                                {

                                    if (P1å¡”[2] >= 2)
                                    {

                                        standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);
                                        towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);

                                    }

                                    else
                                    {

                                        if (P1å¡”[0] >= 3)
                                        {

                                            standbyPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);
                                            towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);

                                        }

                                        if (P1å¡”[1] >= 3)
                                        {

                                            standbyPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);
                                            towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);

                                        }

                                    }

                                }

                            }

                        }

                        if (Enable_Developer_Mode)
                        {

                            accessory.Method.SendChat($"""
                                                       /e 
                                                       standbyPosition={standbyPosition}
                                                       towerPosition={towerPosition}

                                                       """);

                        }

                        if (standbyPosition.Equals(new Vector3(100, 0, 100)) || towerPosition.Equals(new Vector3(100, 0, 100)))
                        {

                            return;

                        }

                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Towers_Position_Thunder_ND";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = standbyPosition;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 10500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Towers_Thunder_ND";
                        dp.Scale = new(4);
                        dp.Position = towerPosition;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 10500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

                    }
                }
                else
                {
                    var eastTower = P1å¡”[3] == 1;
                    //Fire
                    if (myIndex == 0 || myIndex == 1)
                    {
                        var dx2 = eastTower ? -2f : 2f;
                        var dx1 = eastTower ? -5.5f : 5.5f;
                        var dy = myIndex == 0 ? -5.5f : 5.5f;
                        // The expression was myindex == 1 ? -5.5f : 5.5f before. Same background as the previous one.
                        // The bug fix here credits to @alexandria_prime. Appreciate!

                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Towers_Position_Fire_T1";
                        // The name of the drawing here was once incorrectly labeled as towers with thunder Burnt Strike.
                        // Same situation for the other four names below.
                        // The corrections credit to @alexandria_prime. Appreciate!
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = new(100 + dx1, 0, 100 + dy);
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 6500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Towers_Position_Fire_T2";
                        dp.Scale = new(2);
                        dp.Position = new(100 + dx1, 0, 100 + dy);
                        dp.TargetPosition = new(100 + dx2, 0, 100 + dy);
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 6500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Towers_Position_Fire_T3";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = new(100 + dx2, 0, 100 + dy);
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.Delay = 6500;
                        dp.DestoryAt = 1700;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    }
                    else
                    {
                        var myTowerIndex = myIndex - 1;
                        Vector3 standbyPosition = new Vector3(100, 0, 100);
                        Vector3 towerPosition = new Vector3(100, 0, 100);

                        if (Phase1_Strat_Of_Towers == Phase1_Strats_Of_Towers.Completely_Based_On_Priority_å®Œå…¨æ ¹æ®ä¼˜å…ˆçº§_èŽ«çµå–µ)
                        {

                            if (myTowerIndex > 0 && myTowerIndex <= P1å¡”[0]) standbyPosition = new(eastTower ? 102f : 98f, 0, 90.81f);
                            if (myTowerIndex > P1å¡”[0] && myTowerIndex <= P1å¡”[0] + P1å¡”[1]) standbyPosition = new(eastTower ? 102f : 98f, 0, 100f);
                            if (myTowerIndex > P1å¡”[0] + P1å¡”[1] && myTowerIndex <= P1å¡”[0] + P1å¡”[1] + P1å¡”[2]) standbyPosition = new(eastTower ? 102f : 98f, 0, 109.18f);

                            if (myTowerIndex > 0 && myTowerIndex <= P1å¡”[0]) towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);
                            if (myTowerIndex > P1å¡”[0] && myTowerIndex <= P1å¡”[0] + P1å¡”[1]) towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);
                            if (myTowerIndex > P1å¡”[0] + P1å¡”[1] && myTowerIndex <= P1å¡”[0] + P1å¡”[1] + P1å¡”[2]) towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);

                        }

                        if (Phase1_Strat_Of_Towers == Phase1_Strats_Of_Towers.Fixed_H1H2R2_Priority_For_Rest_å›ºå®šH1H2D4å‰©ä½™äººä¼˜å…ˆçº§)
                        {

                            bool fixedPartyMember = false;

                            if (myIndex == 2)
                            {

                                fixedPartyMember = true;

                                standbyPosition = new(eastTower ? 102f : 98f, 0, 90.81f);
                                towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);

                            }

                            if (myIndex == 3)
                            {

                                fixedPartyMember = true;

                                standbyPosition = new(eastTower ? 102f : 98f, 0, 100f);
                                towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);

                            }

                            if (myIndex == 7)
                            {

                                fixedPartyMember = true;

                                standbyPosition = new(eastTower ? 102f : 98f, 0, 109.18f);
                                towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);

                            }

                            if (!fixedPartyMember)
                            {

                                int newTower0 = P1å¡”[0] - 1;
                                int newTower1 = P1å¡”[1] - 1;
                                int newTower2 = P1å¡”[2] - 1;
                                int myNewTowerIndex = myIndex - 3;

                                if (Enable_Developer_Mode)
                                {

                                    accessory.Method.SendChat($"""
                                                               /e 
                                                               newTower0={newTower0}
                                                               newTower1={newTower1}
                                                               newTower2={newTower2}
                                                               myNewTowerIndex={myNewTowerIndex}

                                                               """);

                                }

                                if (newTower0 > 0
                                    &&
                                    0 < myNewTowerIndex && myNewTowerIndex <= newTower0)
                                {

                                    standbyPosition = new(eastTower ? 102f : 98f, 0, 90.81f);
                                    towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);

                                }

                                if (newTower1 > 0
                                   &&
                                   newTower0 < myNewTowerIndex && myNewTowerIndex <= newTower0 + newTower1)
                                {

                                    standbyPosition = new(eastTower ? 102f : 98f, 0, 100f);
                                    towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);

                                }

                                if (newTower2 > 0
                                   &&
                                   newTower0 + newTower1 < myNewTowerIndex && myNewTowerIndex <= newTower0 + newTower1 + newTower2)
                                {

                                    standbyPosition = new(eastTower ? 102f : 98f, 0, 109.18f);
                                    towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);

                                }

                            }

                        }

                        if (Phase1_Strat_Of_Towers == Phase1_Strats_Of_Towers.Fixed_H1H2R2_Rest_Fill_Vacancies_å›ºå®šH1H2D4å‰©ä½™äººè¡¥ä½_MMW)
                        {
                            // Same as before, the following credits to @abigseal. Appreciate!

                            bool fixedPartyMember = false;

                            if (myIndex == 2)
                            {

                                fixedPartyMember = true;

                                standbyPosition = new(eastTower ? 102f : 98f, 0, 90.81f);
                                towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);

                            }

                            if (myIndex == 3)
                            {

                                fixedPartyMember = true;

                                standbyPosition = new(eastTower ? 102f : 98f, 0, 100f);
                                towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);

                            }

                            if (myIndex == 7)
                            {

                                fixedPartyMember = true;

                                standbyPosition = new(eastTower ? 102f : 98f, 0, 109.18f);
                                towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);

                            }

                            if (!fixedPartyMember)
                            {

                                if (Enable_Developer_Mode)
                                {

                                    accessory.Method.SendChat($"""
                                                               /e 
                                                               P1å¡”[0]={P1å¡”[0]}
                                                               P1å¡”[1]={P1å¡”[1]}
                                                               P1å¡”[2]={P1å¡”[2]}
                                                               myTowerIndex={myTowerIndex}

                                                               """);

                                }

                                if (myIndex == 4)
                                {

                                    if (P1å¡”[0] >= 2)
                                    {

                                        standbyPosition = new(eastTower ? 102f : 98f, 0, 90.81f);
                                        towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);

                                    }

                                    else
                                    {

                                        if (P1å¡”[1] >= 3)
                                        {

                                            standbyPosition = new(eastTower ? 102f : 98f, 0, 100f);
                                            towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);

                                        }

                                        if (P1å¡”[2] >= 3)
                                        {

                                            standbyPosition = new(eastTower ? 102f : 98f, 0, 109.18f);
                                            towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);

                                        }

                                    }

                                }

                                if (myIndex == 5)
                                {

                                    if (P1å¡”[1] >= 2)
                                    {

                                        standbyPosition = new(eastTower ? 102f : 98f, 0, 100f);
                                        towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);

                                    }

                                    else
                                    {

                                        if (P1å¡”[0] >= 3)
                                        {

                                            standbyPosition = new(eastTower ? 102f : 98f, 0, 90.81f);
                                            towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);

                                        }

                                        if (P1å¡”[2] >= 3)
                                        {

                                            standbyPosition = new(eastTower ? 102f : 98f, 0, 109.18f);
                                            towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);

                                        }

                                    }

                                }

                                if (myIndex == 6)
                                {

                                    if (P1å¡”[2] >= 2)
                                    {

                                        standbyPosition = new(eastTower ? 102f : 98f, 0, 109.18f);
                                        towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 109.18f);

                                    }

                                    else
                                    {

                                        if (P1å¡”[0] >= 3)
                                        {

                                            standbyPosition = new(eastTower ? 102f : 98f, 0, 90.81f);
                                            towerPosition = new(eastTower ? 113.08f : 86.92f, 0, 90.81f);

                                        }

                                        if (P1å¡”[1] >= 3)
                                        {

                                            standbyPosition = new(eastTower ? 102f : 98f, 0, 100f);
                                            towerPosition = new(eastTower ? 115.98f : 84.02f, 0, 100f);

                                        }

                                    }

                                }

                            }

                        }

                        if (Enable_Developer_Mode)
                        {

                            accessory.Method.SendChat($"""
                                                       /e 
                                                       standbyPosition={standbyPosition}
                                                       towerPosition={towerPosition}

                                                       """);

                        }

                        if (standbyPosition.Equals(new Vector3(100, 0, 100)) || towerPosition.Equals(new Vector3(100, 0, 100)))
                        {

                            return;

                        }

                        var dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Towers_Position_Fire_ND";
                        dp.Scale = new(2);
                        dp.Owner = accessory.Data.Me;
                        dp.TargetPosition = standbyPosition;
                        dp.ScaleMode |= ScaleMode.YByDistance;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 9000;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                        dp = accessory.Data.GetDefaultDrawProperties();
                        dp.Name = "P1_Towers_Fire_ND";
                        dp.Scale = new(4);
                        dp.Position = towerPosition;
                        dp.Color = accessory.Data.DefaultSafeColor;
                        dp.DestoryAt = 10500;
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);

                    }
                }
            });

        }

        #endregion

        #region Phase_2

        [ScriptMethod(name: "----- Phase 2 ----- (No actual meaning for this toggle)",
            eventType: EventTypeEnum.NpcYell,
            eventCondition: ["Your poor",
                            "ç»™æˆ‘ä½ ä»¬è´«ç©·çš„äºº"])]

        public void Phase2_Placeholder(Event @event, ScriptAccessory accessory) { }

        [ScriptMethod(name: "P2_Transition", eventType: EventTypeEnum.Director, eventCondition: ["Instance:800375BF", "Command:8000001E"], userControl: false)]
        public void P2_Transition(Event @event, ScriptAccessory accessory)
        {
            parse=2;
        }

        [ScriptMethod(name: "Phase2 Diamond Dust Initialization",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40180"],
            userControl: false)]

        public void Phase2_Diamond_Dust_Initialization_é’»çŸ³æ˜Ÿå°˜åˆå§‹åŒ–(Event @event, ScriptAccessory accessory)
        {

            parse=21;

            phase2_bossId = @event["SourceId"];
            Phase2_Positions_Of_Icicle_Impact.Clear();
            phase2_positionToBeKnockedBack = new Vector3(100, 0, 100);
            phase2_semaphoreOfGuidanceBeforeKnockback = new System.Threading.AutoResetEvent(false);
            phase2_semaphoreOfGuidanceAfterKnockback = new System.Threading.AutoResetEvent(false);

        }

        [ScriptMethod(name: "P2_DiamondDust_CircleDonutRecord", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4020[23]))$"], userControl: false)]
        public void P2_DiamondDust_CircleDonutRecord(Event @event, ScriptAccessory accessory)
        {
            P2DDDircle = (@event["ActionId"] == "40202");//circle
        }
        [ScriptMethod(name: "P2_DiamondDust_CircleDonut", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4020[23]))$"])]
        public void P2_DiamondDust_CircleDonut(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (@event["ActionId"] == "40202")//circle
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_DiamondDust_Circle";
                dp.Scale = new(16);
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
            else
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_DiamondDust_Donut";
                dp.Scale = new(20);
                dp.InnerScale = new(4);
                dp.Radian = float.Pi * 2;
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 6000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);
            }
        }
        [ScriptMethod(name: "P2_DiamondDust_ConeBait", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^((4020[23]))$"])]
        public void P2_DiamondDust_ConeBait(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dur = 3000;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_ConeBait1";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_ConeBait2";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_ConeBait3";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 3;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_ConeBait4";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Owner = sid;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);


        }
        [ScriptMethod(name: "P2_DiamondDust_IciclePosition", eventType: EventTypeEnum.TargetIcon)]
        public void P2_DiamondDust_IciclePosition(Event @event, ScriptAccessory accessory)
        {
            //accessory.Log.Debug($"{ParsTargetIcon(@event["Id"])}");
            if (ParsTargetIcon(@event["Id"]) != 127) return;
            if (parse!=21) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var rot = myIndex switch
            {
                0 => 6,
                1 => 0,
                2 => 4,
                3 => 2,
                4 => 4,
                5 => 2,
                6 => 6,
                7 => 0,
                _ => 0,
            };
            Vector3 epos1 = P2DDDircle ? new(119.5f, 0, 100.0f) : new(103.5f, 0, 100.0f);
            Vector3 epos2 = P2DDDircle ? new(119.5f, 0, 100.0f) : new(108.0f, 0, 100.0f);
            var dir8 = Phase2_Positions_Of_Icicle_Impact.FirstOrDefault() % 4;
            var dr = dir8 == 0 || dir8 == 2 ? -1 : 0;
            var dealpos1 = RotatePoint(epos1, new(100, 0, 100), float.Pi / 4 * (rot + dr));
            var dealpos2 = RotatePoint(epos2, new(100, 0, 100), float.Pi / 4 * (rot + dr));
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_IciclePosition1";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos1;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_IciclePosition2";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Position = dealpos1;
            dp.TargetPosition = dealpos2;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_IciclePosition3";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos2;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 5500;
            dp.DestoryAt = 2500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "Phase2 Frigid Needle",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40199"])]

        public void Phase2_Frigid_Needle_å†°é’ˆ(Event @event, ScriptAccessory accessory)
        {

            if (parse!=21)
            {

                return;

            }

            Vector3 center = JsonConvert.DeserializeObject<Vector3>(@event["EffectPosition"]);
            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            for (int i = 0; i <= 7; ++i)
            {

                currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase2_Frigid_Needle";
                currentProperty.Scale = new(5, 40);
                currentProperty.Position = center;
                currentProperty.Color = accessory.Data.DefaultDangerColor;
                currentProperty.Rotation = (float.Pi / 4) * i;
                currentProperty.Delay = 3250;
                currentProperty.DestoryAt = 4000;

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

            }

        }

        [ScriptMethod(name: "P2_DiamondDust_ConeBaitPosition", eventType: EventTypeEnum.TargetIcon)]
        public void P2_DiamondDust_ConeBaitPosition(Event @event, ScriptAccessory accessory)
        {
            //accessory.Log.Debug($"{ParsTargetIcon(@event["Id"])}");
            if (ParsTargetIcon(@event["Id"]) != 127) return;
            if (parse!=21) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            int[] group = [6, 7, 4, 5, 2, 3, 0, 1];
            if (accessory.Data.PartyList.IndexOf(((uint)tid)) != group[myIndex]) return;
            var rot = myIndex switch
            {
                0 => 6,
                1 => 0,
                2 => 4,
                3 => 2,
                4 => 4,
                5 => 2,
                6 => 6,
                7 => 0,
                _ => 0,
            };
            var dir8 = Phase2_Positions_Of_Icicle_Impact.FirstOrDefault() % 4;
            var dr = dir8 == 0 || dir8 == 2 ? 0 : -1;
            Vector3 epos = P2DDDircle ? new(116.5f, 0, 100f) : new(101f, 0, 100f);
            var dealpos = RotatePoint(epos, new(100, 0, 100), float.Pi / 4 * (rot + dr));
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_ConeBaitPosition";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 6500;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }

        [ScriptMethod(name: "Phase2 Record Positions Of Icicle Impact",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40198"],
            userControl: false)]

        public void Phase2_Record_Positions_Of_Icicle_Impact_è®°å½•å†°æŸ±å†²å‡»çš„ä½ç½®(Event @event, ScriptAccessory accessory)
        {

            if (parse!=21)
            {

                return;

            }

            Vector3 currentPositions = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            int proteanPosition = PositionTo8Dir(currentPositions, new(100, 0, 100));

            lock (Phase2_Positions_Of_Icicle_Impact)
            {

                Phase2_Positions_Of_Icicle_Impact.Add(proteanPosition);

            }

            if (Enable_Developer_Mode)
            {

                accessory.Method.SendChat($"""
                                           /e 
                                           currentPositions={currentPositions}
                                           proteanPosition={proteanPosition}
                                           
                                           """);

            }

        }

        [ScriptMethod(name: "Phase2 Determine The Position To Be Knocked Back",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40199"],
            userControl: false,
            suppress: 2000)]

        public void Phase2_Determine_The_Position_To_Be_Knocked_Back_ç¡®å®šå‡»é€€ä½ç½®(Event @event, ScriptAccessory accessory)
        {

            if (parse!=21)
            {

                return;

            }

            if (Phase2_Positions_Of_Icicle_Impact.Count == 0)
            {

                return;

            }

            int firstIcicleImpact = Phase2_Positions_Of_Icicle_Impact.First() % 4;
            bool inStGroup = ((int[])[1, 3, 5, 7]).Contains(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
            int rotation = firstIcicleImpact switch
            {
                0 => 2,
                1 => -1,
                2 => 0,
                3 => 1,
            };
            rotation += ((inStGroup) ? (4) : (0));

            phase2_positionToBeKnockedBack = RotatePoint(new Vector3(95, 0, 100), new(100, 0, 100), float.Pi / 4 * rotation);

            System.Threading.Thread.MemoryBarrier();

            phase2_semaphoreOfGuidanceBeforeKnockback.Set();
            phase2_semaphoreOfGuidanceAfterKnockback.Set();

        }

        [ScriptMethod(name: "Phase2 Guidance Of The Position To Be Knocked Back",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40199"],
            suppress: 2000)]

        public void Phase2_Guidance_Of_The_Position_To_Be_Knocked_Back_å‡»é€€ä½ç½®æŒ‡è·¯(Event @event, ScriptAccessory accessory)
        {

            if (parse!=21)
            {

                return;

            }

            if (Phase2_Positions_Of_Icicle_Impact.Count == 0)
            {

                return;

            }

            System.Threading.Thread.MemoryBarrier();

            phase2_semaphoreOfGuidanceBeforeKnockback.WaitOne();

            System.Threading.Thread.MemoryBarrier();

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase2_Guidance_Of_The_Position_To_Be_Knocked_Back";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.TargetPosition = phase2_positionToBeKnockedBack;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.DestoryAt = 4500;

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

        }

        [ScriptMethod(name: "Phase2 Guidance After Knockback",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40208"])]

        public void Phase2_Guidance_After_Knockback_å‡»é€€åŽæŒ‡è·¯(Event @event, ScriptAccessory accessory)
        {

            if (parse!=21)
            {

                return;

            }

            if (Phase2_Positions_Of_Icicle_Impact.Count == 0)
            {

                return;

            }

            System.Threading.Thread.MemoryBarrier();

            phase2_semaphoreOfGuidanceAfterKnockback.WaitOne();

            System.Threading.Thread.MemoryBarrier();

            Vector3 positionOfTheReflection = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            int proteanPositionOfTheReflection = PositionTo8Dir(positionOfTheReflection, new(100, 0, 100));
            int proteanPositionOfTheCurrentGroup = PositionTo8Dir(phase2_positionToBeKnockedBack, new(100, 0, 100));
            int proteanPositionOfTheOppositeGroup = phase2_getOppositeProteanPosition(proteanPositionOfTheCurrentGroup);
            bool propertyHasBeenConfirmed = false;
            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            string prompt = "";

            if (Enable_Developer_Mode)
            {

                accessory.Method.SendChat($"""
                                           /e 
                                           positionOfTheReflection={positionOfTheReflection}
                                           proteanPositionOfTheReflection={proteanPositionOfTheReflection}
                                           proteanPositionOfTheCurrentGroup={proteanPositionOfTheCurrentGroup}
                                           proteanPositionOfTheOppositeGroup={proteanPositionOfTheOppositeGroup}
                                           
                                           """);

            }

            currentProperty.Name = "Phase2_Guidance_After_Knockback";
            currentProperty.Scale = new(20);
            currentProperty.InnerScale = new(19);
            currentProperty.Position = new Vector3(100, 0, 100);
            currentProperty.Rotation = float.Pi - (float.Pi / 4 * proteanPositionOfTheCurrentGroup);
            currentProperty.Color = accessory.Data.DefaultSafeColor.WithW(25f);
            currentProperty.DestoryAt = 14250;

            if (Phase2_Strat_After_Knockback == Phase2_Strats_After_Knockback.Clockwise_One_Group_Counterclockwise_æ€»æ˜¯é¡ºæ—¶é’ˆå•ç»„é€†æ—¶é’ˆ)
            {

                if (((proteanPositionOfTheCurrentGroup + 1) % 8) == proteanPositionOfTheReflection)
                {

                    currentProperty.Radian = float.Pi / 2 - float.Pi / 18;
                    currentProperty.Rotation += (float.Pi / 2 - float.Pi / 18) / 2;

                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                    {

                        prompt = "é€†æ—¶é’ˆ80åº¦,é‡è§å¯¹ç»„";

                    }

                    if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                    {

                        prompt = "Counterclockwise 80 degrees, encountering the opposite group";

                    }

                }

                else
                {

                    if (((proteanPositionOfTheOppositeGroup + 1) % 8) == proteanPositionOfTheReflection)
                    {

                        currentProperty.Radian = float.Pi / 2 - float.Pi / 18;
                        currentProperty.Rotation += -((float.Pi / 2 - float.Pi / 18) / 2);

                        if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                        {

                            prompt = "é¡ºæ—¶é’ˆ80åº¦,é‡è§å¯¹ç»„";

                        }

                        if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                        {

                            prompt = "Clockwise 80 degrees, encountering the opposite group";

                        }

                    }

                    else
                    {

                        int rotationOfThePath = 1;

                        while (((proteanPositionOfTheCurrentGroup + rotationOfThePath) % 8) != proteanPositionOfTheReflection
                              &&
                              ((proteanPositionOfTheCurrentGroup + rotationOfThePath) % 8) != phase2_getOppositeProteanPosition(proteanPositionOfTheReflection))
                        {

                            ++rotationOfThePath;

                        }

                        currentProperty.Radian = float.Pi / 4 * rotationOfThePath;
                        currentProperty.Rotation += -((float.Pi / 4 * rotationOfThePath) / 2);

                        rotationOfThePath *= 45;

                        if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                        {

                            prompt = $"é¡ºæ—¶é’ˆ{rotationOfThePath}åº¦";

                        }

                        if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                        {

                            prompt = $"Clockwise {rotationOfThePath} degrees";

                        }

                    }

                }

                propertyHasBeenConfirmed = true;

            }

            if (Phase2_Strat_After_Knockback == Phase2_Strats_After_Knockback.Counterclockwise_One_Group_Clockwise_æ€»æ˜¯é€†æ—¶é’ˆå•ç»„é¡ºæ—¶é’ˆ)
            {

                if (((proteanPositionOfTheCurrentGroup - 1 + 8) % 8) == proteanPositionOfTheReflection)
                {

                    currentProperty.Radian = float.Pi / 2 - float.Pi / 18;
                    currentProperty.Rotation += -((float.Pi / 2 - float.Pi / 18) / 2);

                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                    {

                        prompt = "é¡ºæ—¶é’ˆ80åº¦,é‡è§å¯¹ç»„";

                    }

                    if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                    {

                        prompt = "Clockwise 80 degrees, encountering the opposite group";

                    }

                }

                else
                {

                    if (((proteanPositionOfTheOppositeGroup - 1 + 8) % 8) == proteanPositionOfTheReflection)
                    {

                        currentProperty.Radian = float.Pi / 2 - float.Pi / 18;
                        currentProperty.Rotation += (float.Pi / 2 - float.Pi / 18) / 2;

                        if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                        {

                            prompt = "é€†æ—¶é’ˆ80åº¦,é‡è§å¯¹ç»„";

                        }

                        if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                        {

                            prompt = "Counterclockwise 80 degrees, encountering the opposite group";

                        }

                    }

                    else
                    {

                        int rotationOfThePath = 1;

                        while (((proteanPositionOfTheCurrentGroup - rotationOfThePath + 8) % 8) != proteanPositionOfTheReflection
                              &&
                              ((proteanPositionOfTheCurrentGroup - rotationOfThePath + 8) % 8) != phase2_getOppositeProteanPosition(proteanPositionOfTheReflection))
                        {

                            ++rotationOfThePath;

                        }

                        currentProperty.Radian = float.Pi / 4 * rotationOfThePath;
                        currentProperty.Rotation += (float.Pi / 4 * rotationOfThePath) / 2;

                        rotationOfThePath *= 45;

                        if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                        {

                            prompt = $"é€†æ—¶é’ˆ{rotationOfThePath}åº¦";

                        }

                        if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                        {

                            prompt = $"Counterclockwise {rotationOfThePath} degrees";

                        }

                    }

                }

                propertyHasBeenConfirmed = true;

            }

            if (Phase2_Strat_After_Knockback == Phase2_Strats_After_Knockback.Clockwise_Both_Groups_Counterclockwise_æ€»æ˜¯é¡ºæ—¶é’ˆåŒç»„é€†æ—¶é’ˆ_èŽ«çµå–µä¸ŽMMW)
            {

                if (((proteanPositionOfTheCurrentGroup + 1) % 8) == proteanPositionOfTheReflection
                   ||
                   ((proteanPositionOfTheOppositeGroup + 1) % 8) == proteanPositionOfTheReflection)
                {

                    currentProperty.Radian = float.Pi / 4 * 3;
                    currentProperty.Rotation += (float.Pi / 4 * 3) / 2;

                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                    {

                        prompt = "é€†æ—¶é’ˆ135åº¦";

                    }

                    if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                    {

                        prompt = "Counterclockwise 135 degrees";

                    }

                }

                else
                {

                    int rotationOfThePath = 1;

                    while (((proteanPositionOfTheCurrentGroup + rotationOfThePath) % 8) != proteanPositionOfTheReflection
                          &&
                          ((proteanPositionOfTheCurrentGroup + rotationOfThePath) % 8) != phase2_getOppositeProteanPosition(proteanPositionOfTheReflection))
                    {

                        ++rotationOfThePath;

                    }

                    currentProperty.Radian = float.Pi / 4 * rotationOfThePath;
                    currentProperty.Rotation += -((float.Pi / 4 * rotationOfThePath) / 2);

                    rotationOfThePath *= 45;

                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                    {

                        prompt = $"é¡ºæ—¶é’ˆ{rotationOfThePath}åº¦";

                    }

                    if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                    {

                        prompt = $"Clockwise {rotationOfThePath} degrees";

                    }

                }

                propertyHasBeenConfirmed = true;

            }

            if (Phase2_Strat_After_Knockback == Phase2_Strats_After_Knockback.Counterclockwise_Both_Groups_Clockwise_æ€»æ˜¯é€†æ—¶é’ˆåŒç»„é¡ºæ—¶é’ˆ)
            {

                if (((proteanPositionOfTheCurrentGroup - 1 + 8) % 8) == proteanPositionOfTheReflection
                   ||
                   ((proteanPositionOfTheOppositeGroup - 1 + 8) % 8) == proteanPositionOfTheReflection)
                {

                    currentProperty.Radian = float.Pi / 4 * 3;
                    currentProperty.Rotation += -((float.Pi / 4 * 3) / 2);

                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                    {

                        prompt = "é¡ºæ—¶é’ˆ135åº¦";

                    }

                    if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                    {

                        prompt = "Clockwise 135 degrees";

                    }

                }

                else
                {

                    int rotationOfThePath = 1;

                    while (((proteanPositionOfTheCurrentGroup - rotationOfThePath + 8) % 8) != proteanPositionOfTheReflection
                          &&
                          ((proteanPositionOfTheCurrentGroup - rotationOfThePath + 8) % 8) != phase2_getOppositeProteanPosition(proteanPositionOfTheReflection))
                    {

                        ++rotationOfThePath;

                    }

                    currentProperty.Radian = float.Pi / 4 * rotationOfThePath;
                    currentProperty.Rotation += (float.Pi / 4 * rotationOfThePath) / 2;

                    rotationOfThePath *= 45;

                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                    {

                        prompt = $"é€†æ—¶é’ˆ{rotationOfThePath}åº¦";

                    }

                    if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                    {

                        prompt = $"Counterclockwise {rotationOfThePath} degrees";

                    }

                }

                propertyHasBeenConfirmed = true;

            }

            if (propertyHasBeenConfirmed)
            {

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, currentProperty);

            }

            if (!prompt.Equals(""))
            {

                if (Enable_Text_Prompts)
                {

                    accessory.Method.TextInfo(prompt, 9000);

                }

                if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
                {

                    accessory.TTS(prompt, Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

            }

            if (!ParseObjectId(@event["SourceId"], out var sourceId))
            {

                return;

            }

            currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase2_Front_Central_Axis_Of_Oracles_Reflection";
            currentProperty.Scale = new(0.5f, 50f);
            currentProperty.Owner = sourceId;
            currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(25f);
            currentProperty.DestoryAt = 14250;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase2_Rear_Separator_Of_Oracles_Reflection";
            currentProperty.Scale = new(0.3f, 10f);
            currentProperty.Owner = sourceId;
            currentProperty.Rotation = float.Pi / 4 * 3;
            currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(25f);
            currentProperty.DestoryAt = 14250;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase2_Rear_Separator_Of_Oracles_Reflection";
            currentProperty.Scale = new(0.3f, 10f);
            currentProperty.Owner = sourceId;
            currentProperty.Rotation = -(float.Pi / 4 * 3);
            currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(25f);
            currentProperty.DestoryAt = 14250;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

        }

        private int phase2_getOppositeProteanPosition(int currentProteanPosition)
        {

            return currentProteanPosition switch
            {
                0 => 4,
                1 => 5,
                2 => 6,
                3 => 7,
                4 => 0,
                5 => 1,
                6 => 2,
                7 => 3,
                _ => currentProteanPosition
            };

        }

        [ScriptMethod(name: "Phase2 Prediction Of Skating",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40208"])]

        public void Phase2_Prediction_Of_Skating_æ»‘å†°é¢„æµ‹(Event @event, ScriptAccessory accessory)
        {

            if (parse!=21)
            {

                return;

            }

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase2_Prediction_Of_Skating";
            currentProperty.Scale = new(2f, 32f);
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(3f);
            currentProperty.Delay = 14250;
            currentProperty.DestoryAt = 9000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, currentProperty);

        }

        [ScriptMethod(name: "P2_DiamondDust_BladeRange", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^4019[34]$"])]
        public void P2_DiamondDust_BladeRange(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var time = 300;
            //93 front first
            if (@event["ActionId"] == "40193")
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_DiamondDust_BladeRangeFront1";
                dp.Scale = new(30);
                dp.Radian = float.Pi / 2 * 3;
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 3500 - time;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_DiamondDust_BladeRangeBack2";
                dp.Scale = new(30);
                dp.Radian = float.Pi / 2;
                dp.Rotation = float.Pi;
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = 3500 - time;
                dp.DestoryAt = 2000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
            else
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_DiamondDust_BladeRangeBack1";
                dp.Scale = new(30);
                dp.Radian = float.Pi / 2;
                dp.Rotation = float.Pi;
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 3500 - time;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P2_DiamondDust_BladeRangeFront2";
                dp.Scale = new(30);
                dp.Radian = float.Pi / 2 * 3;
                dp.Owner = sid;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = 3500 - time;
                dp.DestoryAt = 2000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
        }
        [ScriptMethod(name: "P2_DiamondDust_BossAway", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^40208$", "TargetIndex:1"])]
        public void P2_DiamondDust_BossAway(Event @event, ScriptAccessory accessory)
        {
            if (parse!=21) return;
            if (!ParseObjectId(phase2_bossId, out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_DiamondDust_BossAway";
            dp.Scale = new(5);
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.SightAvoid, dp);


        }

        [ScriptMethod(name: "Phase2 Reset Semaphores After Diamond Dust",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40210"],
            userControl: false)]

        public void Phase2_Reset_Semaphores_After_Diamond_Dust_é’»çŸ³æ˜Ÿå°˜åŽé‡ç½®ä¿¡å·ç¯(Event @event, ScriptAccessory accessory)
        {

            if (parse!=21
               &&
               parse!=22)
            {

                return;

            }

            phase2_semaphoreOfGuidanceBeforeKnockback = new System.Threading.AutoResetEvent(false);
            phase2_semaphoreOfGuidanceAfterKnockback = new System.Threading.AutoResetEvent(false);

        }

        [ScriptMethod(name: "Phase2 Mirror Mirror Initialization",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40179"],
            userControl: false)]

        public void Phase2_Mirror_Mirror_Initialization_é•œä¸­å¥‡é‡åˆå§‹åŒ–(Event @event, ScriptAccessory accessory)
        {

            parse=22;

            phase2_proteanPositionOfTheColourlessMirror = -1;
            phase2_semaphoreTheColourlessMirrorWasConfirmed = new System.Threading.AutoResetEvent(false);
            phase2_proteanPositionsOfRedMirrors.Clear();
            phase2_semaphoreRedMirrorsWereConfirmed = new System.Threading.AutoResetEvent(false);

        }

        [ScriptMethod(name: "P2_MirrorMirror_SpreadStack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4022[01])$"])]
        public void P2_MirrorMirror_SpreadStack(Event @event, ScriptAccessory accessory)
        {
            if (parse!=22) return;
            string prompt = "";
            if (@event["ActionId"] == "40221")
            {
                foreach (var pm in accessory.Data.PartyList)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_MirrorMirror_Spread";
                    dp.Scale = new(5);
                    dp.Owner = pm;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    prompt = "åˆ†æ•£";

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    prompt = "Spread";

                }

            }
            else
            {
                //int[] group = [6, 7, 4, 5, 2, 3, 0, 1];
                int[] group = [4, 5, 6, 7, 0, 1, 2, 3];
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                for (int i = 0; i < 4; i++)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_MirrorMirror_Stack";
                    dp.Scale = new(5);
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = group[myindex] == i || i == myindex ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    prompt = "åˆ†æ‘Š";

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    prompt = "Stack";

                }

            }

            if (!prompt.Equals(""))
            {

                if (Enable_Text_Prompts)
                {

                    accessory.Method.TextInfo(prompt, 1500);

                }

                if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
                {

                    accessory.TTS(prompt, Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

            }

        }
        [ScriptMethod(name: "P2_MirrorMirror_ColourlessDonutAndCone", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375BF", "State:00020001"])]
        public void P2_MirrorMirror_ColourlessDonutAndCone(Event @event, ScriptAccessory accessory)
        {
            if (parse!=22) return;
            if (!int.TryParse(@event["Index"], out var dir8)) return;
            Vector3 npos = new(100, 0, 80);
            dir8--;
            Vector3 dealpos = RotatePoint(npos, new(100, 0, 100), float.Pi / 4 * dir8);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_MirrorMirror_ColourlessDonut";
            dp.Scale = new(20);
            dp.InnerScale = new(4);
            dp.Radian = float.Pi * 2;
            dp.Position = dealpos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6000;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_MirrorMirror_ColourlessConeBait1";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6000;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_MirrorMirror_ColourlessConeBait2";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6000;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_MirrorMirror_ColourlessConeBait3";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 3;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6000;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_MirrorMirror_ColourlessConeBait4";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 6000;
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }
        [ScriptMethod(name: "P2_MirrorMirror_RedDonutAndCone", eventType: EventTypeEnum.EnvControl, eventCondition: ["DirectorId:800375BF", "State:02000100"])]
        public void P2_MirrorMirror_RedDonutAndCone(Event @event, ScriptAccessory accessory)
        {
            if (parse!=22) return;
            if (!int.TryParse(@event["Index"], out var dir8)) return;
            Vector3 npos = new(100, 0, 80);
            dir8--;
            Vector3 dealpos = RotatePoint(npos, new(100, 0, 100), float.Pi / 4 * dir8);
            var dur = 4000;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_MirrorMirror_RedDonut";
            dp.Scale = new(20);
            dp.InnerScale = new(4);
            dp.Radian = float.Pi * 2;
            dp.Position = dealpos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 17000;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Donut, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_MirrorMirror_RedConeBait1";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 1;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 23000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_MirrorMirror_RedConeBait2";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 23000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_MirrorMirror_RedConeBait3";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 3;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 23000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_MirrorMirror_RedConeBait4";
            dp.Scale = new(60);
            dp.Radian = float.Pi / 6;
            dp.Position = dealpos;
            dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp.TargetOrderIndex = 4;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 23000 - dur;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

        }

        [ScriptMethod(name: "Phase2 Determine The Protean Position Of The Colourless Mirror",
            eventType: EventTypeEnum.EnvControl,
            eventCondition: ["DirectorId:800375BF", "State:00020001"],
            userControl: false)]

        public void Phase2_Determine_The_Protean_Position_Of_The_Colourless_Mirror_ç¡®å®šæ— è‰²é•œå­å…«æ–¹ä½ç½®(Event @event, ScriptAccessory accessory)
        {

            if (parse!=22)
            {

                return;

            }

            if (!int.TryParse(@event["Index"], out var proteanPosition))
            {

                return;

            }

            --proteanPosition;
            // The values of Index, which is from 1 to 8, coincidentally correspond to north, northeast, east, ..., northwest.

            phase2_proteanPositionOfTheColourlessMirror = proteanPosition;

            System.Threading.Thread.MemoryBarrier();

            phase2_semaphoreTheColourlessMirrorWasConfirmed.Set();

        }

        [ScriptMethod(name: "Phase2 Rough Guidance Of The Colourless Mirror",
            eventType: EventTypeEnum.EnvControl,
            eventCondition: ["DirectorId:800375BF", "State:00020001"])]

        public void Phase2_Rough_Guidance_Of_The_Colourless_Mirror_æ— è‰²é•œå­ç²—ç•¥æŒ‡è·¯(Event @event, ScriptAccessory accessory)
        {

            if (parse!=22)
            {

                return;

            }

            if (!int.TryParse(@event["Index"], out var proteanPosition))
            {

                return;

            }

            --proteanPosition;

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            Vector3 rawPosition = new(100, 0, 100);
            bool isMeleeGroup = true;

            if (myIndex == 0
               ||
               myIndex == 1
               ||
               myIndex == 4
               ||
               myIndex == 5)
            {

                isMeleeGroup = true;
                rawPosition = new(100, 0, 85);

            }

            if (myIndex == 2
               ||
               myIndex == 3
               ||
               myIndex == 6
               ||
               myIndex == 7)
            {

                isMeleeGroup = false;
                rawPosition = new(100, 0, 80.5f);

            }

            if (rawPosition.Equals(new Vector3(100, 0, 100)))
            {

                return;

            }

            Vector3 targetPosition = RotatePoint(rawPosition, new(100, 0, 100), float.Pi / 4 * (proteanPosition + ((isMeleeGroup) ? (4) : (0))));

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase2_Rough_Guidance_Of_The_Colourless_Mirror";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.TargetPosition = targetPosition;
            currentProperty.Color = Phase2_Colour_Of_Mirror_Rough_Guidance.V4.WithW(1f);
            currentProperty.DestoryAt = 13000;

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            if (!ParseObjectId(phase2_bossId, out var bossId))
            {

                return;

            }

            currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase2_Potential_Dangerous_Zone_Of_The_Colourless_Mirror";
            currentProperty.Scale = new(4);
            currentProperty.Radian = float.Pi;
            currentProperty.Owner = bossId;
            currentProperty.TargetPosition = RotatePoint(new Vector3(100,0,80),new Vector3(100,0,100),float.Pi/4*proteanPosition);
            currentProperty.Color = Phase2_Colour_Of_Potential_Dangerous_Zones.V4.WithW(3f);
            currentProperty.Delay = 6000;
            currentProperty.DestoryAt = 7000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase2_Potential_Dangerous_Zone_Of_The_Colourless_Mirror";
            currentProperty.Scale = new(4);
            currentProperty.Radian = float.Pi / 3;
            currentProperty.Position = RotatePoint(new Vector3(100,0,80),new Vector3(100,0,100),float.Pi/4*proteanPosition);
            currentProperty.TargetObject = bossId;
            currentProperty.Color = Phase2_Colour_Of_Potential_Dangerous_Zones.V4.WithW(3f);
            currentProperty.Delay = 6000;
            currentProperty.DestoryAt = 7000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, currentProperty);

        }

        [ScriptMethod(name: "Phase2 Determine Protean Positions Of Red Mirrors",
            eventType: EventTypeEnum.EnvControl,
            eventCondition: ["DirectorId:800375BF", "State:02000100"],
            userControl: false)]

        public void Phase2_Determine_Protean_Positions_Of_Red_Mirrors_ç¡®å®šçº¢è‰²é•œå­å…«æ–¹ä½ç½®(Event @event, ScriptAccessory accessory)
        {

            if (parse!=22)
            {

                return;

            }

            if (!int.TryParse(@event["Index"], out var proteanPosition))
            {

                return;

            }

            --proteanPosition;

            lock (phase2_proteanPositionsOfRedMirrors)
            {

                if (phase2_proteanPositionsOfRedMirrors.Count < 2)
                {

                    phase2_proteanPositionsOfRedMirrors.Add(proteanPosition);

                }

                if (phase2_proteanPositionsOfRedMirrors.Count == 2)
                {

                    phase2_semaphoreRedMirrorsWereConfirmed.Set();

                }

            }

        }

        [ScriptMethod(name: "Phase2 Rough Guidance Of Red Mirrors",
            eventType: EventTypeEnum.EnvControl,
            eventCondition: ["DirectorId:800375BF", "State:02000100"],
            suppress: 2000)]

        public void Phase2_Rough_Guidance_Of_Red_Mirrors_çº¢è‰²é•œå­ç²—ç•¥æŒ‡è·¯(Event @event, ScriptAccessory accessory)
        {

            if (parse!=22)
            {

                return;

            }

            System.Threading.Thread.MemoryBarrier();

            phase2_semaphoreTheColourlessMirrorWasConfirmed.WaitOne();
            phase2_semaphoreRedMirrorsWereConfirmed.WaitOne();

            System.Threading.Thread.MemoryBarrier();

            if (phase2_proteanPositionOfTheColourlessMirror == -1)
            {

                return;

            }

            int colourlessMirror = phase2_proteanPositionOfTheColourlessMirror;

            if (phase2_proteanPositionsOfRedMirrors.Count != 2)
            {

                return;

            }

            int redMirror1 = phase2_proteanPositionsOfRedMirrors[0];
            int redMirror2 = phase2_proteanPositionsOfRedMirrors[1];
            int discreteDistanceToTheNext = 1;
            int leftMirror = -1;
            int rightMirror = -1;

            while (((redMirror1 + discreteDistanceToTheNext) % 8) != redMirror2)
            {

                ++discreteDistanceToTheNext;

            }

            if (discreteDistanceToTheNext != 2 && discreteDistanceToTheNext != 6)
            {

                return;

            }

            if (discreteDistanceToTheNext == 2)
            {

                leftMirror = redMirror1;
                rightMirror = redMirror2;

            }

            if (discreteDistanceToTheNext == 6)
            {

                leftMirror = redMirror2;
                rightMirror = redMirror1;

            }

            if (leftMirror == -1 || rightMirror == -1)
            {

                return;

            }

            if (Enable_Developer_Mode)
            {

                accessory.Method.SendChat($"""
                                           /e 
                                           leftMirror={leftMirror}
                                           rightMirror={rightMirror}
                                           discreteDistanceToTheNext={discreteDistanceToTheNext}
                                           
                                           """);

            }

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            bool isMeleeGroup = true;

            if (myIndex == 0
               ||
               myIndex == 1
               ||
               myIndex == 4
               ||
               myIndex == 5)
            {

                isMeleeGroup = true;

            }

            if (myIndex == 2
               ||
               myIndex == 3
               ||
               myIndex == 6
               ||
               myIndex == 7)
            {

                isMeleeGroup = false;

            }

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            if (((leftMirror + 1) % 8) == colourlessMirror
               ||
               ((leftMirror + 1) % 8) == phase2_getOppositeProteanPosition(colourlessMirror))
            {

                if (Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Left_Red_è¿‘æˆ˜ç»„åŽ»å·¦çº¢è‰²é•œå­
                   ||
                   Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Closest_Red_Left_If_Same_è¿‘æˆ˜ç»„æœ€è¿‘çº¢è‰²é•œå­è·ç¦»ç›¸åŒåˆ™å·¦)
                {

                    Vector3 targetPosition = new Vector3(100, 0, 100);

                    if (isMeleeGroup)
                    {

                        targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * leftMirror);

                    }

                    else
                    {

                        targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * rightMirror);

                    }

                    currentProperty = accessory.Data.GetDefaultDrawProperties();

                    currentProperty.Name = "Phase2_Rough_Guidance_Of_Red_Mirrors";
                    currentProperty.Scale = new(2);
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Owner = accessory.Data.Me;
                    currentProperty.TargetPosition = targetPosition;
                    currentProperty.Color = Phase2_Colour_Of_Mirror_Rough_Guidance.V4.WithW(1f);
                    currentProperty.Delay = 13500;
                    currentProperty.DestoryAt = 9500;

                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                }

                if (Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Right_Red_è¿‘æˆ˜ç»„åŽ»å³çº¢è‰²é•œå­
                   ||
                   Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Closest_Red_Right_If_Same_è¿‘æˆ˜ç»„æœ€è¿‘çº¢è‰²é•œå­è·ç¦»ç›¸åŒåˆ™å³_èŽ«çµå–µä¸ŽMMW)
                {

                    Vector3 targetPosition = new Vector3(100, 0, 100);

                    if (isMeleeGroup)
                    {

                        targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * rightMirror);

                    }

                    else
                    {

                        targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * leftMirror);

                    }

                    currentProperty = accessory.Data.GetDefaultDrawProperties();

                    currentProperty.Name = "Phase2_Rough_Guidance_Of_Red_Mirrors";
                    currentProperty.Scale = new(2);
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Owner = accessory.Data.Me;
                    currentProperty.TargetPosition = targetPosition;
                    currentProperty.Color = Phase2_Colour_Of_Mirror_Rough_Guidance.V4.WithW(1f);
                    currentProperty.Delay = 13500;
                    currentProperty.DestoryAt = 9500;

                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                }

            }

            else
            {

                if (Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Left_Red_è¿‘æˆ˜ç»„åŽ»å·¦çº¢è‰²é•œå­)
                {

                    Vector3 targetPosition = new Vector3(100, 0, 100);

                    if (isMeleeGroup)
                    {

                        targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * leftMirror);

                    }

                    else
                    {

                        targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * rightMirror);

                    }

                    currentProperty = accessory.Data.GetDefaultDrawProperties();

                    currentProperty.Name = "Phase2_Rough_Guidance_Of_Red_Mirrors";
                    currentProperty.Scale = new(2);
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Owner = accessory.Data.Me;
                    currentProperty.TargetPosition = targetPosition;
                    currentProperty.Color = Phase2_Colour_Of_Mirror_Rough_Guidance.V4.WithW(1f);
                    currentProperty.Delay = 13500;
                    currentProperty.DestoryAt = 9500;

                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                }

                if (Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Right_Red_è¿‘æˆ˜ç»„åŽ»å³çº¢è‰²é•œå­)
                {

                    Vector3 targetPosition = new Vector3(100, 0, 100);

                    if (isMeleeGroup)
                    {

                        targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * rightMirror);

                    }

                    else
                    {

                        targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * leftMirror);

                    }

                    currentProperty = accessory.Data.GetDefaultDrawProperties();

                    currentProperty.Name = "Phase2_Rough_Guidance_Of_Red_Mirrors";
                    currentProperty.Scale = new(2);
                    currentProperty.ScaleMode |= ScaleMode.YByDistance;
                    currentProperty.Owner = accessory.Data.Me;
                    currentProperty.TargetPosition = targetPosition;
                    currentProperty.Color = Phase2_Colour_Of_Mirror_Rough_Guidance.V4.WithW(1f);
                    currentProperty.Delay = 13500;
                    currentProperty.DestoryAt = 9500;

                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                }

                int meleeGroup = phase2_getOppositeProteanPosition(colourlessMirror);
                int discreteDistanceToTheLeft = 0;
                int discreteDistanceToTheRight = 0;

                while (((meleeGroup + discreteDistanceToTheLeft) % 8) != leftMirror)
                {

                    ++discreteDistanceToTheLeft;

                }

                while (((meleeGroup - discreteDistanceToTheRight + 8) % 8) != rightMirror)
                {

                    ++discreteDistanceToTheRight;

                }

                if (Enable_Developer_Mode)
                {

                    accessory.Method.SendChat($"""
                                               /e 
                                               discreteDistanceToTheLeft={discreteDistanceToTheLeft}
                                               discreteDistanceToTheRight={discreteDistanceToTheRight}

                                               """);

                }

                if (discreteDistanceToTheLeft < discreteDistanceToTheRight)
                {

                    if (Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Closest_Red_Left_If_Same_è¿‘æˆ˜ç»„æœ€è¿‘çº¢è‰²é•œå­è·ç¦»ç›¸åŒåˆ™å·¦
                       ||
                       Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Closest_Red_Right_If_Same_è¿‘æˆ˜ç»„æœ€è¿‘çº¢è‰²é•œå­è·ç¦»ç›¸åŒåˆ™å³_èŽ«çµå–µä¸ŽMMW)
                    {

                        Vector3 targetPosition = new Vector3(100, 0, 100);

                        if (isMeleeGroup)
                        {

                            targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * leftMirror);

                        }

                        else
                        {

                            targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * rightMirror);

                        }

                        currentProperty = accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name = "Phase2_Rough_Guidance_Of_Red_Mirrors";
                        currentProperty.Scale = new(2);
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = targetPosition;
                        currentProperty.Color = Phase2_Colour_Of_Mirror_Rough_Guidance.V4.WithW(1f);
                        currentProperty.Delay = 13500;
                        currentProperty.DestoryAt = 9500;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                    }

                }

                if (discreteDistanceToTheLeft > discreteDistanceToTheRight)
                {

                    if (Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Closest_Red_Left_If_Same_è¿‘æˆ˜ç»„æœ€è¿‘çº¢è‰²é•œå­è·ç¦»ç›¸åŒåˆ™å·¦
                       ||
                       Phase2_Strat_Of_Mirror_Mirror == Phase2_Strats_Of_Mirror_Mirror.Melee_Group_Closest_Red_Right_If_Same_è¿‘æˆ˜ç»„æœ€è¿‘çº¢è‰²é•œå­è·ç¦»ç›¸åŒåˆ™å³_èŽ«çµå–µä¸ŽMMW)
                    {

                        Vector3 targetPosition = new Vector3(100, 0, 100);

                        if (isMeleeGroup)
                        {

                            targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * rightMirror);

                        }

                        else
                        {

                            targetPosition = RotatePoint(new Vector3(100, 0, 80.5f), new(100, 0, 100), float.Pi / 4 * leftMirror);

                        }

                        currentProperty = accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name = "Phase2_Rough_Guidance_Of_Red_Mirrors";
                        currentProperty.Scale = new(2);
                        currentProperty.ScaleMode |= ScaleMode.YByDistance;
                        currentProperty.Owner = accessory.Data.Me;
                        currentProperty.TargetPosition = targetPosition;
                        currentProperty.Color = Phase2_Colour_Of_Mirror_Rough_Guidance.V4.WithW(1f);
                        currentProperty.Delay = 13500;
                        currentProperty.DestoryAt = 9500;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                    }

                }

            }

            currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase2_Potential_Dangerous_Zone_Of_Red_Mirrors";
            currentProperty.Scale = new(4);
            currentProperty.Radian = float.Pi / 3;
            currentProperty.Position = RotatePoint(new(100, 0, 80), new(100, 0, 100), float.Pi / 4 * leftMirror);
            currentProperty.Rotation = float.Pi / 6;
            currentProperty.TargetPosition = new Vector3(100, 0, 100);
            currentProperty.Color = Phase2_Colour_Of_Potential_Dangerous_Zones.V4.WithW(3f);
            currentProperty.Delay = 13500;
            currentProperty.DestoryAt = 10000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase2_Potential_Dangerous_Zone_Of_Red_Mirrors";
            currentProperty.Scale = new(4);
            currentProperty.Radian = float.Pi / 3;
            currentProperty.Position = RotatePoint(new(100, 0, 80), new(100, 0, 100), float.Pi / 4 * rightMirror);
            currentProperty.Rotation = -(float.Pi / 6);
            currentProperty.TargetPosition = new Vector3(100, 0, 100);
            currentProperty.Color = Phase2_Colour_Of_Potential_Dangerous_Zones.V4.WithW(3f);
            currentProperty.Delay = 13500;
            currentProperty.DestoryAt = 10000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, currentProperty);

        }

        [ScriptMethod(name: "Phase2 Reset Semaphores After Mirror Mirror",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40212"],
            userControl: false)]

        public void Phase2_Reset_Semaphores_After_Mirror_Mirror_é•œä¸­å¥‡é‡åŽé‡ç½®ä¿¡å·ç¯(Event @event, ScriptAccessory accessory)
        {

            if (parse!=22
               &&
               parse!=23)
            {

                return;

            }

            phase2_semaphoreTheColourlessMirrorWasConfirmed = new System.Threading.AutoResetEvent(false);
            phase2_semaphoreRedMirrorsWereConfirmed = new System.Threading.AutoResetEvent(false);

        }

        [ScriptMethod(name: "Phase2 Light Rampant Initialization",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40212"],
            userControl: false)]

        public void Phase2_Light_Rampant_Initialization_å…‰ä¹‹å¤±æŽ§åˆå§‹åŒ–(Event @event, ScriptAccessory accessory)
        {

            parse=23;

            phase2_playersWithLuminousHammer.Clear();
            phase2_semaphoreLuminousHammerWasConfirmed = new System.Threading.AutoResetEvent(false);
            phase2_stacksOfLightsteeped = [0, 0, 0, 0, 0, 0, 0, 0];
            phase2_writePermissionForLightsteeped = true;
            phase2_semaphoreFinalLightsteepedWasConfirmed = new System.Threading.AutoResetEvent(false);

        }

        [ScriptMethod(name: "Phase2 Initial Positions Before Light Rampant",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40212"])]

        public void Phase2_Initial_Positions_Before_Light_Rampant_å…‰ä¹‹å¤±æŽ§å‰åˆå§‹ç«™ä½(Event @event, ScriptAccessory accessory)
        {

            if (parse!=22
               &&
               parse!=23)
            {

                return;

            }

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            double rotation = 0d;

            if (Phase2_Initial_Protean_Position_Of_Light_Rampant == Phase2_Initial_Protean_Positions_Of_Light_Rampant.Normal_Protean_Tanks_North_East_For_Both_Grey9_å¸¸è§„å…«æ–¹Tåœ¨ä¸œåŒ—_ç°9ç”¨)
            {

                rotation = 0d;

                rotation += myIndex switch
                {
                    0 => 0d,
                    7 => 1d,
                    1 => 2d,
                    5 => 3d,
                    3 => 4d,
                    4 => 5d,
                    2 => 6d,
                    6 => 7d
                };

            }

            if (Phase2_Initial_Protean_Position_Of_Light_Rampant == Phase2_Initial_Protean_Positions_Of_Light_Rampant.Supporters_North_MOTH12_For_JPPF_And_L_è“ç»¿å…¨éƒ¨åœ¨åŒ—MSTH12_æ—¥é‡Žå’ŒLå›¢ç”¨)
            {

                rotation = -0.5d;

                rotation += myIndex switch
                {
                    0 => -1d,
                    1 => 0d,
                    2 => 1d,
                    3 => 2d,
                    7 => 3d,
                    6 => 4d,
                    5 => 5d,
                    4 => 6d
                };

            }

            if (Phase2_Initial_Protean_Position_Of_Light_Rampant == Phase2_Initial_Protean_Positions_Of_Light_Rampant.Supporters_North_H12MOT_For_JPPF_And_L_è“ç»¿å…¨éƒ¨åœ¨åŒ—H12MST_æ—¥é‡Žå’ŒLå›¢ç”¨)
            {

                rotation = -0.5d;

                rotation += myIndex switch
                {
                    2 => -1d,
                    3 => 0d,
                    0 => 1d,
                    1 => 2d,
                    7 => 3d,
                    6 => 4d,
                    5 => 5d,
                    4 => 6d
                };

            }

            var currentproperty = accessory.Data.GetDefaultDrawProperties();

            currentproperty.Name = "Phase2_Initial_Positions_Before_Light_Rampant";
            currentproperty.Scale = new(2);
            currentproperty.Owner = accessory.Data.Me;
            currentproperty.TargetPosition = RotatePoint(new Vector3(100, 0, 95), new Vector3(100, 0, 100), (float)(float.Pi / 4 * rotation)); ;
            currentproperty.ScaleMode |= ScaleMode.YByDistance;
            currentproperty.Color = accessory.Data.DefaultSafeColor;
            currentproperty.DestoryAt = 5000;

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

        }

        [ScriptMethod(name: "Phase2 Rough Path Of Luminous Hammer",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40212"],
            suppress: 2000)]

        public void Phase2_Rough_Path_Of_Luminous_Hammer_å…‰æµä¾µèš€å¤§è‡´è·¯å¾„(Event @event, ScriptAccessory accessory)
        {

            if (parse!=23)
            {

                return;

            }

            var currentproperty = accessory.Data.GetDefaultDrawProperties();

            if (Phase2_Strat_Of_Light_Rampant == Phase2_Strats_Of_Light_Rampant.Star_Of_David_Japanese_PF_å…­èŠ’æ˜Ÿæ—¥æœé‡Žé˜Ÿæ³•_èŽ«çµå–µä¸ŽMMW)
            {

                Vector3 point1 = new Vector3(97.321f, 0f, 106.467f);
                Vector3 point1Symmetry = RotatePoint(point1, new Vector3(100, 0, 100), float.Pi);
                Vector3 point2 = new Vector3(93f, 0f, 100f);
                Vector3 point2Symmetry = RotatePoint(point2, new Vector3(100, 0, 100), float.Pi);
                Vector3 point3 = new Vector3(97.321f, 0f, 93.533f);
                Vector3 point3Symmetry = RotatePoint(point3, new Vector3(100, 0, 100), float.Pi);
                Vector3 point4 = new Vector3(97.321f, 0f, 82f);
                Vector3 point4Symmetry = RotatePoint(point4, new Vector3(100, 0, 100), float.Pi);

                currentproperty = accessory.Data.GetDefaultDrawProperties();

                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point1;
                currentproperty.TargetPosition = point2;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();

                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point2;
                currentproperty.TargetPosition = point3;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();

                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point3;
                currentproperty.TargetPosition = point4;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();

                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point1Symmetry;
                currentproperty.TargetPosition = point2Symmetry;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();

                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point2Symmetry;
                currentproperty.TargetPosition = point3Symmetry;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();

                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point3Symmetry;
                currentproperty.TargetPosition = point4Symmetry;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

            }

            if (Phase2_Strat_Of_Light_Rampant == Phase2_Strats_Of_Light_Rampant.New_Grey9_æ–°ç°ä¹æ³•_èŽ«çµå–µä¸ŽMMW)
            {

                Vector3 point1 = new Vector3(92f, 0f, 100f);
                Vector3 point1Symmetry = RotatePoint(point1, new Vector3(100, 0, 100), float.Pi);
                Vector3 point2 = new Vector3(94.343f, 0f, 94.343f);
                Vector3 point2Symmetry = RotatePoint(point2, new Vector3(100, 0, 100), float.Pi);
                Vector3 point3 = new Vector3(100f, 0f, 92f);
                Vector3 point3Symmetry = RotatePoint(point3, new Vector3(100, 0, 100), float.Pi);
                Vector3 point4 = new Vector3(106.133f, 0f, 91.97f);
                Vector3 point4Symmetry = RotatePoint(point4, new Vector3(100, 0, 100), float.Pi);
                Vector3 point5 = new Vector3(111.314f, 0f, 88.686f);
                Vector3 point5Symmetry = RotatePoint(point5, new Vector3(100, 0, 100), float.Pi);
				
                currentproperty = accessory.Data.GetDefaultDrawProperties();

                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point1;
                currentproperty.TargetPosition = point2;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();

                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point2;
                currentproperty.TargetPosition = point3;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();

                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point3;
                currentproperty.TargetPosition = point4;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();
				
                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point4;
                currentproperty.TargetPosition = point5;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();
				
                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point1Symmetry;
                currentproperty.TargetPosition = point2Symmetry;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();

                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point2Symmetry;
                currentproperty.TargetPosition = point3Symmetry;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

                currentproperty = accessory.Data.GetDefaultDrawProperties();

                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point3Symmetry;
                currentproperty.TargetPosition = point4Symmetry;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);
				
				currentproperty = accessory.Data.GetDefaultDrawProperties();
				
                currentproperty.Name = "Phase2_Rough_Path_Of_Luminous_Hammer";
                currentproperty.Scale = new(2);
                currentproperty.Position = point4Symmetry;
                currentproperty.TargetPosition = point5Symmetry;
                currentproperty.ScaleMode |= ScaleMode.YByDistance;
                currentproperty.Color = Phase2_Colour_Of_Rough_Paths.V4.WithW(1f);
                currentproperty.Delay = 3500;
                currentproperty.DestoryAt = 9500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentproperty);

            }

        }

        [ScriptMethod(name: "Phase2 Determine Luminous Hammer During Light Rampant",
            eventType: EventTypeEnum.TargetIcon,
            userControl: false)]

        public void Phase2_Determine_Luminous_Hammer_During_Light_Rampant_å…‰ä¹‹å¤±æŽ§ç¡®å®šå…‰æµä¾µèš€(Event @event, ScriptAccessory accessory)
        {
            if (ParsTargetIcon(@event["Id"]) != 157)
            {

                return;

            }

            if (parse!=23)
            {

                return;

            }

            if (!ParseObjectId(@event["TargetId"], out var targetId))
            {

                return;

            }

            int currentIndex = accessory.Data.PartyList.IndexOf(((uint)targetId));

            lock (phase2_playersWithLuminousHammer)
            {

                if (phase2_playersWithLuminousHammer.Count < 2)
                {

                    phase2_playersWithLuminousHammer.Add(currentIndex);

                }

                if (phase2_playersWithLuminousHammer.Count == 2)
                {

                    phase2_semaphoreLuminousHammerWasConfirmed.Set();

                }

            }

        }

        [ScriptMethod(name: "Phase2 Determine Stacks Of Lightsteeped During Light Rampant",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:2257"],
            userControl: false)]

        public void Phase2_Determine_Stacks_Of_Lightsteeped_During_Light_Rampant_å…‰ä¹‹å¤±æŽ§ç¡®å®šè¿‡é‡å…‰å±‚æ•°(Event @event, ScriptAccessory accessory)
        {

            if (parse!=23)
            {

                return;

            }

            if (!phase2_writePermissionForLightsteeped)
            {

                return;

            }

            if (!ParseObjectId(@event["TargetId"], out var targetId))
            {

                return;

            }

            if (!int.TryParse(@event["StackCount"], out var stacks))
            {

                return;

            }

            int currentIndex = accessory.Data.PartyList.IndexOf(((uint)targetId));

            lock (phase2_stacksOfLightsteeped)
            {

                phase2_stacksOfLightsteeped[currentIndex] = stacks;

            }

            if (Enable_Developer_Mode)
            {

                accessory.Method.SendChat($"""
                                           /e 
                                           currentIndex={currentIndex}
                                           stacks={stacks}

                                           """);

            }

        }

        [ScriptMethod(name: "Phase2 Disable The Write Permission For Lightsteeped",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40218"],
            userControl: false)]

        public void Phase2_Disable_The_Write_Permission_For_Lightsteeped_ç¦æ­¢å†™å…¥è¿‡é‡å…‰(Event @event, ScriptAccessory accessory)
        {

            phase2_writePermissionForLightsteeped = false;

        }

        [ScriptMethod(name: "P2_LightRampant_SpreadStack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4022[01])$"])]
        public void P2_LightRampant_SpreadStack(Event @event, ScriptAccessory accessory)
        {
            if (parse!=23) return;
            string prompt = "";
            if (@event["ActionId"] == "40221")
            {
                foreach (var pm in accessory.Data.PartyList)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_LightRampant_Spread";
                    dp.Scale = new(5);
                    dp.Owner = pm;
                    dp.Color = accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    prompt = "åˆ†æ•£";

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    prompt = "Spread";

                }

            }
            else
            {
                int[] group = [6, 7, 4, 5, 2, 3, 0, 1];
                var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                for (int i = 0; i < 4; i++)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P2_LightRampant_Stack";
                    dp.Scale = new(5);
                    dp.Owner = accessory.Data.PartyList[i];
                    dp.Color = group[myindex] == i || i == myindex ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
                }

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    prompt = "åˆ†æ‘Š";

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    prompt = "Stack";

                }

            }

            if (!prompt.Equals(""))
            {

                if (Enable_Text_Prompts)
                {

                    accessory.Method.TextInfo(prompt, 1500);

                }

                if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
                {

                    accessory.TTS(prompt, Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

            }

        }
        [ScriptMethod(name: "P2_LightRampant_StackBuff", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:4159"])]
        public void P2_LightRampant_StackBuff(Event @event, ScriptAccessory accessory)
        {
            if (parse!=23) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_LightRampant_StackBuff";
            dp.Scale = new(5);
            dp.Owner = tid;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 12000;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Phase2 Guidance Of Towers During Light Rampant",
            eventType: EventTypeEnum.TargetIcon,
            suppress: 2000)]

        public void Phase2_Guidance_Of_Towers_During_Light_Rampant_å…‰ä¹‹å¤±æŽ§è¸©å¡”æŒ‡è·¯(Event @event, ScriptAccessory accessory)
        {

            if (ParsTargetIcon(@event["Id"]) != 157)
            {

                return;

            }

            if (parse!=23)
            {

                return;

            }

            System.Threading.Thread.MemoryBarrier();

            phase2_semaphoreLuminousHammerWasConfirmed.WaitOne();

            System.Threading.Thread.MemoryBarrier();

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            if (phase2_playersWithLuminousHammer.Contains(myIndex))
            {

                return;

            }

            List<int> playersWithTethers = [];

            if (Phase2_Initial_Protean_Position_Of_Light_Rampant == Phase2_Initial_Protean_Positions_Of_Light_Rampant.Normal_Protean_Tanks_North_East_For_Both_Grey9_å¸¸è§„å…«æ–¹Tåœ¨ä¸œåŒ—_ç°9ç”¨)
            {

                List<int> orderFromTheWestInclusive = [2, 6, 0, 7, 1, 5, 3, 4];

                for (int i = 0; i < orderFromTheWestInclusive.Count; ++i)
                {

                    if (!phase2_playersWithLuminousHammer.Contains(orderFromTheWestInclusive[i]))
                    {

                        playersWithTethers.Add(orderFromTheWestInclusive[i]);

                    }

                }

            }

            if (Phase2_Initial_Protean_Position_Of_Light_Rampant == Phase2_Initial_Protean_Positions_Of_Light_Rampant.Supporters_North_MOTH12_For_JPPF_And_L_è“ç»¿å…¨éƒ¨åœ¨åŒ—MSTH12_æ—¥é‡Žå’ŒLå›¢ç”¨)
            {

                List<int> orderFromTheWestInclusive = [0, 1, 2, 3, 7, 6, 5, 4];

                for (int i = 0; i < orderFromTheWestInclusive.Count; ++i)
                {

                    if (!phase2_playersWithLuminousHammer.Contains(orderFromTheWestInclusive[i]))
                    {

                        playersWithTethers.Add(orderFromTheWestInclusive[i]);

                    }

                }

            }

            if (Phase2_Initial_Protean_Position_Of_Light_Rampant == Phase2_Initial_Protean_Positions_Of_Light_Rampant.Supporters_North_H12MOT_For_JPPF_And_L_è“ç»¿å…¨éƒ¨åœ¨åŒ—H12MST_æ—¥é‡Žå’ŒLå›¢ç”¨)
            {

                List<int> orderFromTheWestInclusive = [2, 3, 0, 1, 7, 6, 5, 4];

                for (int i = 0; i < orderFromTheWestInclusive.Count; ++i)
                {

                    if (!phase2_playersWithLuminousHammer.Contains(orderFromTheWestInclusive[i]))
                    {

                        playersWithTethers.Add(orderFromTheWestInclusive[i]);

                    }

                }

            }

            if (Enable_Developer_Mode)
            {

                accessory.Method.SendChat($"""
                                           /e 
                                           playersWithTethers.Count={playersWithTethers.Count}
                                           playersWithTethers[]={playersWithTethers[0]},{playersWithTethers[1]},{playersWithTethers[2]},{playersWithTethers[3]},{playersWithTethers[4]},{playersWithTethers[5]} 
                                           
                                           """);

            }

            int myTetherIndex = playersWithTethers.IndexOf(myIndex);
            Vector3 myTower = new Vector3(100, 0, 100);
            Vector3 myMeetingPoint = new Vector3(100, 0, 100);

            Vector3 tower1 = new Vector3(100.00f, 0, 084.00f);
            // North.
            Vector3 tower2 = new Vector3(113.85f, 0, 092.00f);
            // Northeast.
            Vector3 tower3 = new Vector3(113.85f, 0, 108.00f);
            // Southeast.
            Vector3 tower4 = new Vector3(100.00f, 0, 116.00f);
            // South.
            Vector3 tower5 = new Vector3(086.14f, 0, 108.00f);
            // Southwest.
            Vector3 tower6 = new Vector3(086.14f, 0, 092.00f);
            // Northwest.

            Vector3 northMeetingPoint = new Vector3(100.00f, 0, 82.00f);
            Vector3 eastMeetingPoint = new Vector3(118.00f, 0, 100.00f);
            Vector3 southMeetingPoint = new Vector3(100.00f, 0, 118.00f);
            Vector3 westMeetingPoint = new Vector3(82.00f, 0, 100.00f);

            if (Phase2_Strat_Of_Light_Rampant == Phase2_Strats_Of_Light_Rampant.Star_Of_David_Japanese_PF_å…­èŠ’æ˜Ÿæ—¥æœé‡Žé˜Ÿæ³•_èŽ«çµå–µä¸ŽMMW)
            {
                accessory.Log.Debug("Star_Of_David_Japanese_PF_å…­èŠ’æ˜Ÿæ—¥æœé‡Žé˜Ÿæ³•_èŽ«çµå–µä¸ŽMMW");
                myTower = myTetherIndex switch
                {
                    1 => tower4,
                    4 => tower1,
                    0 => tower6,
                    2 => tower2,
                    3 => tower5,
                    5 => tower3
                };

                if (Vector3.Distance(myTower, tower1) < 1
                   ||
                   Vector3.Distance(myTower, tower2) < 1
                   ||
                   Vector3.Distance(myTower, tower6) < 1)
                {

                    myMeetingPoint = northMeetingPoint;

                }

                else
                {

                    myMeetingPoint = southMeetingPoint;

                }

            }

            if (Phase2_Strat_Of_Light_Rampant == Phase2_Strats_Of_Light_Rampant.New_Grey9_æ–°ç°ä¹æ³•_èŽ«çµå–µä¸ŽMMW)
            {
                foreach (var item in phase2_playersWithLuminousHammer)
                {
                    accessory.Log.Debug($"{item}");
                }
                int numberOfPlayersWithLuminousHammerBefore = 0;

                if (myIndex == 0)
                {

                    if (Enable_Developer_Mode)
                    {

                        accessory.Method.SendChat($"""
                                                   /e 
                                                   numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}

                                                   """);

                    }

                    myTower = tower4;

                }

                numberOfPlayersWithLuminousHammerBefore += (phase2_playersWithLuminousHammer.Contains(0)) ? (1) : (0);

                if (myIndex == 7)
                {

                    if (Enable_Developer_Mode)
                    {

                        accessory.Method.SendChat($"""
                                                   /e 
                                                   numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}

                                                   """);

                    }

                    myTower = (phase2_playersWithLuminousHammer.Contains(0)) ? (tower4) : (tower6);

                }

                numberOfPlayersWithLuminousHammerBefore += (phase2_playersWithLuminousHammer.Contains(7)) ? (1) : (0);

                if (myIndex == 1)
                {

                    if (Enable_Developer_Mode)
                    {

                        accessory.Method.SendChat($"""
                                                   /e 
                                                   numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}

                                                   """);

                    }

                    myTower = numberOfPlayersWithLuminousHammerBefore switch
                    {
                        0 => tower2,
                        1 => tower6,
                        2 => tower4
                    };

                }

                numberOfPlayersWithLuminousHammerBefore += (phase2_playersWithLuminousHammer.Contains(1)) ? (1) : (0);

                if (myIndex == 5)
                {

                    if (Enable_Developer_Mode)
                    {

                        accessory.Method.SendChat($"""
                                                   /e 
                                                   numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}

                                                   """);

                    }

                    myTower = numberOfPlayersWithLuminousHammerBefore switch
                    {
                        0 => tower5,
                        1 => tower2,
                        2 => tower6
                    };

                }

                numberOfPlayersWithLuminousHammerBefore += (phase2_playersWithLuminousHammer.Contains(5)) ? (1) : (0);

                if (myIndex == 3)
                {

                    if (Enable_Developer_Mode)
                    {

                        accessory.Method.SendChat($"""
                                                   /e 
                                                   numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}

                                                   """);

                    }

                    myTower = numberOfPlayersWithLuminousHammerBefore switch
                    {
                        0 => tower3,
                        1 => tower5,
                        2 => tower2
                    };

                }

                numberOfPlayersWithLuminousHammerBefore += (phase2_playersWithLuminousHammer.Contains(3)) ? (1) : (0);

                if (myIndex == 4)
                {

                    if (Enable_Developer_Mode)
                    {

                        accessory.Method.SendChat($"""
                                                   /e 
                                                   numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}

                                                   """);

                    }

                    myTower = numberOfPlayersWithLuminousHammerBefore switch
                    {
                        0 => tower1,
                        1 => tower3,
                        2 => tower5
                    };

                }

                numberOfPlayersWithLuminousHammerBefore += (phase2_playersWithLuminousHammer.Contains(4)) ? (1) : (0);

                if (myIndex == 2)
                {

                    if (Enable_Developer_Mode)
                    {

                        accessory.Method.SendChat($"""
                                                   /e 
                                                   numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}

                                                   """);

                    }

                    myTower = (phase2_playersWithLuminousHammer.Contains(6)) ? (tower1) : (tower3);

                }

                numberOfPlayersWithLuminousHammerBefore += (phase2_playersWithLuminousHammer.Contains(2)) ? (1) : (0);

                if (myIndex == 6)
                {

                    if (Enable_Developer_Mode)
                    {

                        accessory.Method.SendChat($"""
                                                   /e 
                                                   numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}

                                                   """);

                    }

                    myTower = tower1;

                }

                if (Vector3.Distance(myTower, tower2) < 1
                   ||
                   Vector3.Distance(myTower, tower3) < 1
                   ||
                   Vector3.Distance(myTower, tower4) < 1)
                {

                    myMeetingPoint = eastMeetingPoint;

                }

                else
                {

                    myMeetingPoint = westMeetingPoint;

                }

            }

            if (Phase2_Strat_Of_Light_Rampant == Phase2_Strats_Of_Light_Rampant.Lucrezia_Lå›¢æ³•)
            {
                accessory.Log.Debug("Lucrezia_Lå›¢æ³•");
                myTower = myTetherIndex switch
                {
                    1 => tower1,
                    4 => tower4,
                    0 => tower5,
                    2 => tower3,
                    3 => tower6,
                    5 => tower2
                };

                if (Vector3.Distance(myTower, tower1) < 1
                   ||
                   Vector3.Distance(myTower, tower2) < 1
                   ||
                   Vector3.Distance(myTower, tower6) < 1)
                {

                    myMeetingPoint = northMeetingPoint;

                }

                else
                {

                    myMeetingPoint = southMeetingPoint;

                }

            }

            if (Phase2_Strat_Of_Light_Rampant == Phase2_Strats_Of_Light_Rampant.Obsolete_Old_Grey9_å·²æ·˜æ±°çš„æ—§ç°ä¹æ³•_èŽ«çµå–µ)
            {
                accessory.Log.Debug("Obsolete_Old_Grey9_å·²æ·˜æ±°çš„æ—§ç°ä¹æ³•_èŽ«çµå–µ");
                int numberOfPlayersWithLuminousHammerBefore = 0;

                if (myIndex == 0)
                {

                    if (Enable_Developer_Mode)
                    {

                        accessory.Method.SendChat($"""
                                                   /e 
                                                   numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}

                                                   """);

                    }

                    myTower = tower4;

                }

                numberOfPlayersWithLuminousHammerBefore += (phase2_playersWithLuminousHammer.Contains(0)) ? (1) : (0);

                if (myIndex == 7)
                {

                    if (Enable_Developer_Mode)
                    {

                        accessory.Method.SendChat($"""
                                                   /e 
                                                   numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}

                                                   """);

                    }

                    myTower = (phase2_playersWithLuminousHammer.Contains(0)) ? (tower4) : (tower2);

                }

                numberOfPlayersWithLuminousHammerBefore += (phase2_playersWithLuminousHammer.Contains(7)) ? (1) : (0);

                if (myIndex == 1)
                {

                    if (Enable_Developer_Mode)
                    {

                        accessory.Method.SendChat($"""
                                                   /e 
                                                   numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}

                                                   """);

                    }

                    myTower = numberOfPlayersWithLuminousHammerBefore switch
                    {
                        0 => tower6,
                        1 => tower2,
                        2 => tower4
                    };

                }

                numberOfPlayersWithLuminousHammerBefore += (phase2_playersWithLuminousHammer.Contains(1)) ? (1) : (0);

                if (myIndex == 5)
                {

                    if (Enable_Developer_Mode)
                    {

                        accessory.Method.SendChat($"""
                                                   /e 
                                                   numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}

                                                   """);

                    }

                    myTower = numberOfPlayersWithLuminousHammerBefore switch
                    {
                        0 => tower3,
                        1 => tower6,
                        2 => tower2
                    };

                }

                numberOfPlayersWithLuminousHammerBefore += (phase2_playersWithLuminousHammer.Contains(5)) ? (1) : (0);

                if (myIndex == 3)
                {

                    if (Enable_Developer_Mode)
                    {

                        accessory.Method.SendChat($"""
                                                   /e 
                                                   numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}

                                                   """);

                    }

                    myTower = numberOfPlayersWithLuminousHammerBefore switch
                    {
                        0 => tower5,
                        1 => tower3,
                        2 => tower6
                    };

                }

                numberOfPlayersWithLuminousHammerBefore += (phase2_playersWithLuminousHammer.Contains(3)) ? (1) : (0);

                if (myIndex == 4)
                {

                    if (Enable_Developer_Mode)
                    {

                        accessory.Method.SendChat($"""
                                                   /e 
                                                   numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}

                                                   """);

                    }

                    myTower = numberOfPlayersWithLuminousHammerBefore switch
                    {
                        0 => tower1,
                        1 => tower5,
                        2 => tower3
                    };

                }

                numberOfPlayersWithLuminousHammerBefore += (phase2_playersWithLuminousHammer.Contains(4)) ? (1) : (0);

                if (myIndex == 2)
                {

                    if (Enable_Developer_Mode)
                    {

                        accessory.Method.SendChat($"""
                                                   /e 
                                                   numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}

                                                   """);

                    }

                    myTower = (phase2_playersWithLuminousHammer.Contains(6)) ? (tower1) : (tower5);

                }

                numberOfPlayersWithLuminousHammerBefore += (phase2_playersWithLuminousHammer.Contains(2)) ? (1) : (0);

                if (myIndex == 6)
                {

                    if (Enable_Developer_Mode)
                    {

                        accessory.Method.SendChat($"""
                                                   /e 
                                                   numberOfPlayersWithLuminousHammerBefore={numberOfPlayersWithLuminousHammerBefore}

                                                   """);

                    }

                    myTower = tower1;

                }

                if (Vector3.Distance(myTower, tower1) < 1
                   ||
                   Vector3.Distance(myTower, tower2) < 1
                   ||
                   Vector3.Distance(myTower, tower3) < 1)
                {

                    myMeetingPoint = eastMeetingPoint;

                }

                else
                {

                    myMeetingPoint = westMeetingPoint;

                }

            }

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase2_Guidance_1_Of_Towers_During_Light_Rampant";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.TargetPosition = myTower;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.DestoryAt = 10000;

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase2_Highlight_Of_The_Tower_During_Light_Rampant";
            currentProperty.Scale = new(4);
            currentProperty.Position = myTower;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.DestoryAt = 10000;

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase2_Guidance_2_Preview_Of_Towers_During_Light_Rampant";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Position = myTower;
            currentProperty.TargetPosition = myMeetingPoint;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.DestoryAt = 10000;

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase2_Guidance_2_Of_Towers_During_Light_Rampant";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.TargetPosition = myMeetingPoint;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.Delay = 10000;
            currentProperty.DestoryAt = 4000;

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

        }

        [ScriptMethod(name: "Phase2 Determine Final Lightsteeped",
            eventType: EventTypeEnum.EnvControl,
            eventCondition: ["DirectorId:800375BF", "State:00020001", "Index:00000015"],
            userControl: false,
            suppress: 2000)]

        public void Phase2_Determine_Final_Lightsteeped_ç¡®å®šæœ€åŽçš„è¿‡é‡å…‰(Event @event, ScriptAccessory accessory)
        {

            if (parse!=23)
            {

                return;

            }

            lock (phase2_playersWithLuminousHammer)
            {

                for (int i = 0; i < 8; ++i)
                {

                    lock (phase2_stacksOfLightsteeped)
                    {

                        ++phase2_stacksOfLightsteeped[i];

                    }

                }

            }

            lock (phase2_playersWithLuminousHammer)
            {

                for (int i = 0; i < 8; ++i)
                {

                    if (!phase2_playersWithLuminousHammer.Contains(i))
                    {

                        lock (phase2_stacksOfLightsteeped)
                        {

                            ++phase2_stacksOfLightsteeped[i];

                        }

                    }

                }

            }

            System.Threading.Thread.MemoryBarrier();

            phase2_semaphoreFinalLightsteepedWasConfirmed.Set();

            System.Threading.Thread.MemoryBarrier();

            if (Enable_Developer_Mode)
            {

                accessory.Method.SendChat($"""
                                           /e 
                                           phase2_stacksOfLightsteeped[]={phase2_stacksOfLightsteeped[0]},{phase2_stacksOfLightsteeped[1]},{phase2_stacksOfLightsteeped[2]},{phase2_stacksOfLightsteeped[3]},{phase2_stacksOfLightsteeped[4]},{phase2_stacksOfLightsteeped[5]},{phase2_stacksOfLightsteeped[6]},{phase2_stacksOfLightsteeped[7]}

                                           """);

            }

        }

        [ScriptMethod(name: "Phase2 Guidance Of The Last Tower During Light Rampant",
            eventType: EventTypeEnum.EnvControl,
            eventCondition: ["DirectorId:800375BF", "State:00020001", "Index:00000015"])]

        public void Phase2_Guidance_Of_The_Last_Tower_During_Light_Rampant_å…‰ä¹‹å¤±æŽ§è¸©æœ€åŽå¡”æŒ‡è·¯(Event @event, ScriptAccessory accessory)
        {

            if (parse!=23)
            {

                return;

            }

            System.Threading.Thread.MemoryBarrier();

            phase2_semaphoreFinalLightsteepedWasConfirmed.WaitOne();

            System.Threading.Thread.MemoryBarrier();

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            if (phase2_stacksOfLightsteeped[myIndex] < 3)
            {

                var currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase2_Guidance_Of_The_Last_Tower_During_Light_Rampant";
                currentProperty.Scale = new(2);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = new Vector3(100, 0, 100);
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.Delay = 2500;
                currentProperty.DestoryAt = 5500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase2_Highlight_Of_The_Last_Tower_During_Light_Rampant";
                currentProperty.Scale = new(4);
                currentProperty.Position = new Vector3(100, 0, 100);
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.Delay = 2500;
                currentProperty.DestoryAt = 5500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, currentProperty);

            }

            if (Enable_Developer_Mode)
            {

                accessory.Method.SendChat($"""
                                           /e 
                                           phase2_stacksOfLightsteeped[myIndex]={phase2_stacksOfLightsteeped[myIndex]}
                                           
                                           """);

            }

        }

        [ScriptMethod(name: "P2_LightRampant_ProteanPosition", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(4022[01])$"])]
        public void P2_LightRampant_ProteanPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse!=23) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var rot8 = myindex switch
            {
                0 => 0,
                1 => 2,
                2 => 6,
                3 => 4,
                4 => 5,
                5 => 3,
                6 => 7,
                7 => 1,
                _ => 0,
            };
            var mPosEnd = RotatePoint(new(100, 0, 95), new(100, 0, 100), float.Pi / 4 * rot8);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2_LightRampant_ProteanPosition";
            dp.Scale = new(2);
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = mPosEnd;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 9000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

        }

        [ScriptMethod(name: "Phase2 Reset Semaphores After Light Rampant",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40224"],
            userControl: false)]

        public void Phase2_Reset_Semaphores_After_Light_Rampant_å…‰ä¹‹å¤±æŽ§åŽé‡ç½®ä¿¡å·ç¯(Event @event, ScriptAccessory accessory)
        {

            if (parse!=23)
            {

                return;

            }

            phase2_semaphoreLuminousHammerWasConfirmed = new System.Threading.AutoResetEvent(false);
            phase2_semaphoreFinalLightsteepedWasConfirmed = new System.Threading.AutoResetEvent(false);

        }

        [ScriptMethod(name: "P2_LightRampant_SphereExplosionWarning", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40219)$"],
            userControl: true)]
        public void P2_LightRampant_SphereExplosionWarning(Event @ev, ScriptAccessory sa)
        {
            // 
            if (!ParseObjectId(@ev["SourceId"], out var sid)) return;
            ScriptColor ColorRed = new ScriptColor { V4 = new Vector4(1.0f, 0f, 0f, 1.0f) };
            var dp = sa.Data.GetDefaultDrawProperties();
            dp.Name = $"Sphere{sid}";
            dp.Scale = new(11f);
            dp.Owner = sid;
            dp.Color = Phase2_Colour_Of_Sphere_AOEs.V4.WithW(3f);
            dp.Delay = 2500;
            dp.DestoryAt = 2500;
            dp.ScaleMode |= ScaleMode.ByTime;
            sa.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        
        #endregion
        
        #region Phase_2_Intermission

        [ScriptMethod(name: "----- Phase 2.5 ----- (No actual meaning for this toggle)",
            eventType: EventTypeEnum.NpcYell,
            eventCondition: ["Your huddled masses yearning to breathe free",
                            "èœ·ç¼©ç€ç¥ˆç›¼è‡ªç”±å‘¼å¸çš„äºº"])]

        public void Phase2point5_Placeholder(Event @event, ScriptAccessory accessory) { }

        [ScriptMethod(name: "P2.5_DarkCrystalAOE", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40262"])]
        public void P2_DarkCrystalAOE(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P2.5_DarkCrystalAOE";
            dp.Scale = new(50);
            dp.Radian = float.Pi / 9;
            dp.Owner = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Fan, dp);
        }
        
        #endregion

        #region Phase_3

        [ScriptMethod(name: "----- Phase 3 ----- (No actual meaning for this toggle)",
            eventType: EventTypeEnum.NpcYell,
            eventCondition: ["The wretched refuse of your teeming shore",
                            "è¢«ä½ ä»¬çš„ç¹è£æ‹’ä¹‹é—¨å¤–å—è‹¦çš„äºº"])]

        public void Phase3_Placeholder(Event @event, ScriptAccessory accessory) { }

        [ScriptMethod(name: "P3_UltimateRelativity_Transition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40266)$"], userControl: false)]
        public void P3_UltimateRelativity_Transition(Event @event, ScriptAccessory accessory)
        {
            parse=31;
            phase3_bossId = @event["SourceId"];
            P3FireBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            P3WaterBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            P3ReturnBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            P3Lamp = [0, 0, 0, 0, 0, 0, 0, 0];
            P3LampWise = [0, 0, 0, 0, 0, 0, 0, 0];
        }
        [ScriptMethod(name: "P3_UltimateRelativity_BuffRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(2455|2456|2464|2462|2461|2460)$"], userControl: false)]
        public void P3_UltimateRelativity_BuffRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse!=31) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (!float.TryParse(@event["Duration"], out var dur)) return;
            var index = accessory.Data.PartyList.IndexOf(((uint)tid));
            if (index == -1) return;
            //Ice
            if (@event["StatusID"] == "2462")
            {
                lock (P3FireBuff)
                {
                    P3FireBuff[index] = 4;
                }
            }
            //Fire
            if (@event["StatusID"] == "2455")
            {

                var count = 1;
                if (dur > 20) count = 2;
                if (dur > 30) count = 3;
                lock (P3FireBuff)
                {
                    P3FireBuff[index] = count;
                }
            }
            //Return
            if (@event["StatusID"] == "2464")
            {
                var count = 1;
                if (dur > 20) count = 3;
                lock (P3ReturnBuff)
                {
                    P3ReturnBuff[index] = count;
                }
            }
            //Water
            if (@event["StatusID"] == "2461")
            {
                lock (P3WaterBuff)
                {
                    P3WaterBuff[index] = 1;
                }
            }
            //Circle
            if (@event["StatusID"] == "2460")
            {
                lock (P3WaterBuff)
                {
                    P3WaterBuff[index] = 2;
                }
            }
            //Away
            if (@event["StatusID"] == "2456")
            {
                lock (P3WaterBuff)
                {
                    P3WaterBuff[index] = 3;
                }
            }



        }
        [ScriptMethod(name: "P3_UltimateRelativity_LampRecord", eventType: EventTypeEnum.Tether, eventCondition: ["Id:regex:^(0085|0086)$"], userControl: false)]
        public void P3_UltimateRelativity_LampRecord(Event @event, ScriptAccessory accessory)
        {
            //0085 Purple
            //0086 Yellow
            if (parse!=31) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var dir8 = PositionTo8Dir(pos, new(100, 0, 100));
            lock (P3Lamp)
            {
                P3Lamp[dir8] = @event["Id"] == "0086" ? 1 : 2;
            }
        }
        [ScriptMethod(name: "P3_UltimateRelativity_LampDirectionRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2970"], userControl: false)]
        public void P3_UltimateRelativity_LampDirectionRecord(Event @event, ScriptAccessory accessory)
        {
            //buff2970, 13 269 clockwise 92 348 counterclockwise
            if (parse!=31) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
            Vector3 centre = new(100, 0, 100);
            var dir8 = PositionTo8Dir(pos, centre);
            P3LampWise[dir8] = @event["StackCount"] == "92" ? 1 : 0;
        }
        [ScriptMethod(name: "P3_UltimateRelativity_LampAOE", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40235", "TargetIndex:1"])]
        public void P3_UltimateRelativity_LampAOE(Event @event, ScriptAccessory accessory)
        {
            if (parse!=31) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var rot = JsonConvert.DeserializeObject<float>(@event["SourceRotation"]);
            Vector3 centre = new(100, 0, 100);
            var dir8 = PositionTo8Dir(pos, centre);
            var isWise = P3LampWise[dir8] == 1;
            for (int i = 0; i < 9; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_UltimateRelativity_LampAOE";
                dp.Scale = new(5, 50);
                dp.Position = pos;
                dp.Rotation = rot + (i + 1) * float.Pi / 12 * (isWise ? -1 : 1);
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 2000 + (i * 1000);
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);
            }
        }
        [ScriptMethod(name: "P3_UltimateRelativity_BuffPosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40293"])]
        public void P3_UltimateRelativity_BuffPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse!=31) return;
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            if (myIndex == -1) return;
            var myDir8 = MyLampIndex(myIndex);
            //accessory.Log.Debug($"myDir8 {myDir8}");
            if (myDir8 == -1) return;
            var myRot = myDir8 * float.Pi / 4;

            Vector3 centre = new(100, 0, 100);
            Vector3 fireN = new(100, 0, 84.5f);
            Vector3 returnPosN = P3WaterBuff[myIndex] == 2 ? new(100, 0, 91.5f) : new(100, 0, 98);
            Vector3 stopPos = new(100, 0, 101);
            //Fire
            var myFire = P3FireBuff[myIndex];
            //Short Fire
            if (myFire == 1)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_UltimateRelativity_ShortFire_DropFire";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(fireN, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_UltimateRelativity_ShortFire_DropReturn";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(returnPosN, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 7500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_UltimateRelativity_ShortFire_CenterStack";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = centre;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 12500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);


                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_UltimateRelativity_ShortFire_OutputPosition";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(stopPos, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 22500;
                dp.DestoryAt = 15000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }

            //Medium Fire
            if (myFire == 2)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_UltimateRelativity_MediumFire_CenterStack";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = centre;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_UltimateRelativity_MediumFire_DropReturn";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(returnPosN, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 7500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_UltimateRelativity_MediumFire_DropFire";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(fireN, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 12500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_UltimateRelativity_MediumFire_Center";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = centre;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 17500;
                dp.DestoryAt = 10000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_UltimateRelativity_MediumFire_OutputPosition";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(stopPos, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 32500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            }

            //Long Fire
            if (myFire == 3)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_UltimateRelativity_LongFire_CenterStack";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = centre;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_UltimateRelativity_LongFire_CenterStack";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = centre;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 12500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_UltimateRelativity_LongFire_Return";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(returnPosN, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 17500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_UltimateRelativity_LongFire_DropFire";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(fireN, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 22500;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_UltimateRelativity_LongFire_Output";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(stopPos, centre, myRot);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 27500;
                dp.DestoryAt = 10000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }

            if (myFire == 4)
            {
                if (myIndex < 4)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_UltimateRelativity_IceTH_DropIce";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = centre;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 7500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_UltimateRelativity_IceTH_DropReturn";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(returnPosN, centre, myRot);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 7500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_UltimateRelativity_IceTH_CenterStack";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = centre;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 12500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);


                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_UltimateRelativity_IceTH_OutputPosition";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(stopPos, centre, myRot);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 22500;
                    dp.DestoryAt = 15000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                else
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_UltimateRelativity_IceD_CenterStack";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = centre;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 7500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_UltimateRelativity_IceD_CenterStack";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = centre;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 12500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_UltimateRelativity_IceD_Return";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(returnPosN, centre, myRot);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 17500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_UltimateRelativity_IceD_DropIce";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = centre;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 22500;
                    dp.DestoryAt = 5000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P3_UltimateRelativity_LongFire_Output";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(stopPos, centre, myRot);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 27500;
                    dp.DestoryAt = 10000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
            }
        }
        [ScriptMethod(name: "P3_UltimateRelativity_LampPosition", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2970"])]
        public void P3_UltimateRelativity_LampPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse!=31) return;
            //buff2970, 13 269 clockwise 92 348 counterclockwise
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);
            Vector3 centre = new(100, 0, 100);
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var dir8 = PositionTo8Dir(pos, centre);
            Vector3 nPos = @event["StackCount"] == "92" ? new(98, 0, 90) : new(102, 0, 90);
            if (dir8 == MyLampIndex(myIndex))
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P3_UltimateRelativity_LampPosition";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = RotatePoint(nPos, centre, dir8 * float.Pi / 4);
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 4000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
        }

        [ScriptMethod(name: "Phase3 Prompt Before Shell Crusher",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40286"])]

        public void Phase3_Prompt_Before_Shell_Crusher_ç ´ç›¾ä¸€å‡»å‰æç¤º(Event @event, ScriptAccessory accessory)
        {

            if (parse!=31)
            {

                return;

            }

            if (Enable_Text_Prompts)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    accessory.Method.TextInfo("åœºä¸­é›†åˆåˆ†æ‘Š", 3000);

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    accessory.Method.TextInfo("Stack in the center", 3000);

                }

            }

            if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    accessory.TTS("åœºä¸­é›†åˆåˆ†æ‘Š", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    accessory.TTS("Stack in the center", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

            }

        }

        [ScriptMethod(name: "P3_UltimateRelativity_DarkHalo", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40290"])]
        public void P3_UltimateRelativity_DarkHalo(Event @event, ScriptAccessory accessory)
        {
            if (parse!=31) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_UltimateRelativity_DarkHalo";
            dp.Scale = new(20);
            dp.Owner = sid;
            dp.TargetObject = tid;
            dp.Color = myindex == 0 || myindex == 1 ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
        }

        [ScriptMethod(name: "Phase3 Initial Orientation Before The Second Half",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40290"])]

        public void Phase3_Initial_Orientation_Before_The_Second_Half_äºŒè¿å‰çš„åˆå§‹é¢å‘(Event @event, ScriptAccessory accessory)
        {

            if (parse!=31)
            {

                return;

            }

            if (!ParseObjectId(@event["SourceId"], out var sourceId))
            {

                return;

            }

            if (!accessory.Data.EnmityList.TryGetValue(sourceId, out var enmityListOfBoss))
            {

                return;

            }

            if (Enable_Developer_Mode)
            {

                accessory.Method.SendChat($"""
                                           /e 
                                           accessory.Data.Me={accessory.Data.Me}
                                           enmityListOfTheBoss[0]={enmityListOfBoss[0]}

                                           """);

            }

            if (accessory.Data.Me != enmityListOfBoss[0])
            {

                return;

            }

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase3_Initial_Orientation_Before_The_Second_Half";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.TargetPosition = new Vector3(100, 0, 94);
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.DestoryAt = 12500;

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            if (Enable_Text_Prompts)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    accessory.Method.TextInfo("è®©Bossé¢å‘æ­£åŒ—", 12500);

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    accessory.Method.TextInfo("Make the Boss orient to the north", 12500);

                }

            }

            if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    accessory.TTS("è®©Bossé¢å‘æ­£åŒ—", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    accessory.TTS("Make the Boss orient to the north", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

            }

        }

        [ScriptMethod(name: "P3_DelayedEchoes_Transition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40269)$"], userControl: false)]
        public void P3_DelayedEchoes_Transition(Event @event, ScriptAccessory accessory)
        {
            parse=32;
            P3FloorFire = -1;
            phase3_typeOfDarkWaterIii = [
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE,
                Phase3_Types_Of_Dark_Water_III.NONE
            ];
            phase3_marksOfPlayers = [
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1,
                MarkType.Stop1
            ];
            phase3_numberOfDarkWaterIiiHasBeenProcessed = 0;
            phase3_numberOfMarksHaveBeenRecorded = 0;
            phase3_semaphoreMarksHaveBeenRecorded = new System.Threading.AutoResetEvent(false);
            phase3_roundOfDarkWaterIii = 0;
            phase3_rangeSemaphoreOfDarkWaterIii = 0;
            phase3_guidanceSemaphoreOfDarkWaterIii = 0;
            phase3_hasConfirmedInitialSafePositions = false;
            phase3_doubleGroup_initialSafePositionOfTheLeftGroup = new Vector3(100, 0, 100);
            phase3_doubleGroup_initialSafePositionOfTheRightGroup = new Vector3(100, 0, 100);
            phase3_doubleGroup_leftPositionToStackOfTheSecondRound = new Vector3(100, 0, 100);
            phase3_doubleGroup_rightPositionToStackOfTheSecondRound = new Vector3(100, 0, 100);
            phase3_locomotive_initialSafePositionOfTheLeftGroup = new Vector3(100, 0, 100);
            phase3_locomotive_initialSafePositionOfTheRightGroup = new Vector3(100, 0, 100);
            phase3_locomotive_leftPositionToStackOfTheSecondRound = new Vector3(100, 0, 100);
            phase3_locomotive_rightPositionToStackOfTheSecondRound = new Vector3(100, 0, 100);
            phase3_moglinMeow_initialSafePositionOfTheLeftGroup = new Vector3(100, 0, 100);
            phase3_moglinMeow_initialSafePositionOfTheRightGroup = new Vector3(100, 0, 100);
            phase3_moglinMeow_leftPositionToStackOfTheSecondRound = new Vector3(100, 0, 100);
            phase3_moglinMeow_rightPositionToStackOfTheSecondRound = new Vector3(100, 0, 100);
            phase3_finalPositionOfTheBoss = new Vector3(100, 0, 100);
        }

        [ScriptMethod(name: "Phase3 Record Signs On Party Members",
            eventType: EventTypeEnum.Marker,
            userControl: false)]

        public void Phase3_Record_Signs_On_Party_Members_è®°å½•å°é˜Ÿé˜Ÿå‘˜çš„ç›®æ ‡æ ‡è®°(Event @event, ScriptAccessory accessory)
        {

            if (parse!=32)
            {

                return;

            }

            if (!ParseObjectId(@event["TargetId"], out var targetId))
            {

                return;

            }

            if (!int.TryParse(@event["Id"], out var sign))
            {

                return;

            }

            MarkType currentType = sign switch
            {
                1 => MarkType.Attack1,
                2 => MarkType.Attack2,
                3 => MarkType.Attack3,
                4 => MarkType.Attack4,
                6 => MarkType.Bind1,
                7 => MarkType.Bind2,
                8 => MarkType.Bind3,
                11 => MarkType.Square,
                _ => MarkType.Stop1
            };

            int currentIndex = accessory.Data.PartyList.IndexOf(((uint)targetId));

            if (0 <= currentIndex && currentIndex <= 7)
            {

                lock (phase3_marksOfPlayers)
                {

                    phase3_marksOfPlayers[currentIndex] = currentType;

                    ++phase3_numberOfMarksHaveBeenRecorded;

                    System.Threading.Thread.MemoryBarrier();

                    if (phase3_numberOfMarksHaveBeenRecorded == 8)
                    {

                        phase3_semaphoreMarksHaveBeenRecorded.Set();

                    }

                }

            }

        }

        [ScriptMethod(name: "Phase3 Determine Types Of Dark Water III",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:2461"],
            userControl: false)]

        public void Phase3_Determine_Types_Of_Dark_Water_III_ç¡®å®šé»‘æš—ç‹‚æ°´ç±»åž‹(Event @event, ScriptAccessory accessory)
        {

            if (parse!=32)
            {

                return;

            }

            if (!ParseObjectId(@event["TargetId"], out var targetId))
            {

                return;

            }

            int currentIndex = accessory.Data.PartyList.IndexOf(((uint)targetId));
            int duration = Convert.ToInt32(@event["DurationMilliseconds"], 10);

            if (currentIndex < 0 || currentIndex > 7)
            {

                return;

            }

            if (duration > 36000)
            {
                // Actually it's 38000ms (38s), but just in case.

                lock (phase3_typeOfDarkWaterIii)
                {

                    phase3_typeOfDarkWaterIii[currentIndex] = Phase3_Types_Of_Dark_Water_III.LONG;

                }

            }

            else
            {

                if (duration > 27000)
                {
                    // Actually it's 29000ms (29s), but just in case.

                    lock (phase3_typeOfDarkWaterIii)
                    {

                        phase3_typeOfDarkWaterIii[currentIndex] = Phase3_Types_Of_Dark_Water_III.MEDIUM;

                    }

                }

                else
                {

                    if (duration > 8000)
                    {
                        // Actually it's 10000ms (10s), but just in case.

                        lock (phase3_typeOfDarkWaterIii)
                        {

                            phase3_typeOfDarkWaterIii[currentIndex] = Phase3_Types_Of_Dark_Water_III.SHORT;

                        }

                    }

                }

            }

            System.Threading.Thread.MemoryBarrier();

            ++phase3_numberOfDarkWaterIiiHasBeenProcessed;

            System.Threading.Thread.MemoryBarrier();

            if (Enable_Developer_Mode)
            {

                accessory.Method.SendChat($"""
                                           /e 
                                           currentIndex={currentIndex}
                                           duration={duration}
                                           phase3_typeOfDarkWaterIii={phase3_typeOfDarkWaterIii[currentIndex]}
                                           
                                           """);

            }

        }

        [ScriptMethod(name: "Phase3 Prompt Before Dark Water III",
            eventType: EventTypeEnum.StatusAdd,
            eventCondition: ["StatusID:2461"],
            suppress: 2000)]

        public void Phase3_Prompt_Before_Dark_Water_III_æš—é»‘ç‹‚æ°´å‰æç¤º(Event @event, ScriptAccessory accessory)
        {

            if (parse!=32)
            {

                return;

            }

            while (phase3_numberOfDarkWaterIiiHasBeenProcessed < 6)
            {

                System.Threading.Thread.Sleep(1);

            }

            System.Threading.Thread.MemoryBarrier();

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Double_Group_åŒåˆ†ç»„æ³•)
            {

                bool goLeft = phase3_doubleGroup_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
                bool stayInTheGroup = phase3_doubleGroup_shouldStayInTheGroup(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
                string prompt = "";
                var currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase3_Prompt_Before_Dark_Water_III";
                currentProperty.Scale = new(2);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = (goLeft) ? (new Vector3(93, 0, 100)) : (new Vector3(107, 0, 100));
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.DestoryAt = 5000;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                if (goLeft)
                {

                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                    {

                        prompt += "åŽ»å·¦ç»„åˆ†æ‘Šç¬¬ä¸€ç¬¬ä¸‰æ¬¡ï¼Œ";

                    }

                    if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                    {

                        prompt += "Go left for the first and third, ";

                    }

                }

                else
                {

                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                    {

                        prompt += "åŽ»å³ç»„åˆ†æ‘Šç¬¬ä¸€ç¬¬ä¸‰æ¬¡ï¼Œ";

                    }

                    if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                    {

                        prompt += "Go right for the first and third, ";

                    }

                }

                if (stayInTheGroup)
                {

                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                    {

                        prompt += "ç¬¬äºŒæ¬¡ç•™åœ¨æœ¬ç»„";

                    }

                    if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                    {

                        prompt += "stay in the current group for the second";

                    }

                }

                else
                {

                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                    {

                        prompt += "ç¬¬äºŒæ¬¡æ¢åŽ»å¯¹ç»„";

                    }

                    if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                    {

                        prompt += "move to the opposite group for the second";

                    }

                }

                if (Enable_Text_Prompts)
                {

                    accessory.Method.TextInfo(prompt, 4000);

                }

                accessory.TTS($"{prompt}", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.High_Priority_As_Locomotives_è½¦å¤´ä½Žæ¢æ³•_MMW)
            {

                int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                bool goLeft = phase3_locomotive_shouldGoLeft(myIndex);
                string prompt = "";
                var currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase3_Prompt_Before_Dark_Water_III";
                currentProperty.Scale = new(2);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = (goLeft) ? (new Vector3(93, 0, 100)) : (new Vector3(107, 0, 100));
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.DestoryAt = 5000;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                if (goLeft)
                {

                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                    {

                        prompt += "åŽ»å·¦ç»„åˆ†æ‘Šï¼Œ";

                    }

                    if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                    {

                        prompt += "Go left to stack, ";

                    }

                }

                else
                {

                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                    {

                        prompt += "åŽ»å³ç»„åˆ†æ‘Šï¼Œ";

                    }

                    if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                    {

                        prompt += "Go right to stack, ";

                    }

                }

                if (Phase3_Branch_Of_The_Locomotive_Strat == Phase3_Branches_Of_The_Locomotive_Strat.MT_And_M1_As_Locomotives_MTå’ŒD1ä¸ºè½¦å¤´_MMW)
                {

                    if (myIndex != 0 && myIndex != 4)
                    {

                        if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                        {

                            prompt += (goLeft) ? ("è·ŸéšMT") : ("è·ŸéšD1");

                        }

                        if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                        {

                            prompt += (goLeft) ? ("follow MT") : ("follow M1");

                        }

                    }

                    if (myIndex == 0 || myIndex == 4)
                    {

                        if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                        {

                            prompt += "ä½ æ˜¯è½¦å¤´";

                        }

                        if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                        {

                            prompt += "you are the locomotive";

                        }

                    }

                }

                if (Phase3_Branch_Of_The_Locomotive_Strat == Phase3_Branches_Of_The_Locomotive_Strat.Others_As_Locomotives_Chinese_PF_å›½æœé‡Žé˜Ÿäººç¾¤ä¸ºè½¦å¤´)
                {

                    if (myIndex != 0 && myIndex != 4)
                    {

                        if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                        {

                            prompt += "ä½ æ˜¯äººç¾¤è½¦å¤´ä¹‹ä¸€";

                        }

                        if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                        {

                            prompt += "you are one of the locomotives";

                        }

                    }

                    if (myIndex == 0 || myIndex == 4)
                    {

                        if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                        {

                            prompt += "è·Ÿéšç»„å†…äººç¾¤";

                        }

                        if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                        {

                            prompt += "follow others in the group";

                        }

                    }

                }

                if (Enable_Text_Prompts)
                {

                    accessory.Method.TextInfo(prompt, 3500);

                }

                accessory.TTS($"{prompt}", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Moglin_Meow_Or_Baby_Wheelchair_Based_On_Signs_æ ¹æ®ç›®æ ‡æ ‡è®°çš„èŽ«çµå–µæ³•æˆ–å®å®æ¤…æ³•)
            {

                System.Threading.Thread.MemoryBarrier();

                phase3_semaphoreMarksHaveBeenRecorded.WaitOne();

                System.Threading.Thread.MemoryBarrier();

                int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                bool goLeft = phase3_moglinMeow_shouldGoLeft(myIndex);
                string prompt = "";
                var currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase3_Prompt_Before_Dark_Water_III";
                currentProperty.Scale = new(2);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = (goLeft) ? (new Vector3(93, 0, 100)) : (new Vector3(107, 0, 100));
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.DestoryAt = 5000;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                if (goLeft)
                {

                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                    {

                        prompt += "å§‹ç»ˆåŽ»å·¦ç»„åˆ†æ‘Š";

                    }

                    if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                    {

                        prompt += "Always stack on the left";

                    }

                }

                else
                {

                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                    {

                        prompt += "å§‹ç»ˆåŽ»å³ç»„åˆ†æ‘Š";

                    }

                    if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                    {

                        prompt += "Always stack on the right";

                    }

                }

                if (Enable_Text_Prompts)
                {

                    accessory.Method.TextInfo(prompt, 3500);

                }

                accessory.TTS($"{prompt}", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

            }

        }

        [ScriptMethod(name: "Phase3 Release The Semaphore Of Dark Water III",
            eventType: EventTypeEnum.StatusRemove,
            eventCondition: ["StatusID:2458"],
            suppress: 2000,
            userControl: false)]

        public void Phase3_Release_The_Semaphore_Of_Dark_Water_III_é‡Šæ”¾é»‘æš—ç‹‚æ°´çš„ä¿¡å·ç¯(Event @event, ScriptAccessory accessory)
        {

            if (parse!=32)
            {

                return;

            }

            if(@event["SourceId"].Equals("00000000")) {
                // A rare local exception. Please refer to the report on Discord for details.
                // In short, there's a very rare chance that a 2458 status from the entity 00000000 will be applied without valid duration.
                // Therefore, that weird 2458 status will be removed immediately, and the removal will cause incorrect guidance.

                return;

            }

            System.Threading.Thread.MemoryBarrier();

            ++phase3_roundOfDarkWaterIii;

            System.Threading.Thread.MemoryBarrier();

            phase3_rangeSemaphoreOfDarkWaterIii = 1;
            phase3_guidanceSemaphoreOfDarkWaterIii = 1;

        }

        [ScriptMethod(name: "Phase3 Range Of Dark Water III",
            eventType: EventTypeEnum.StatusRemove,
            eventCondition: ["StatusID:2458"],
            suppress: 2000)]

        public void Phase3_Range_Of_Dark_Water_III_é»‘æš—ç‹‚æ°´èŒƒå›´(Event @event, ScriptAccessory accessory)
        {
            
            if(@event["SourceId"].Equals("00000000")) {

                return;

            }
            
            System.Threading.Thread.MemoryBarrier();

            while (System.Threading.Interlocked.CompareExchange(ref phase3_rangeSemaphoreOfDarkWaterIii, 0, 1) == 0)
            {

                System.Threading.Thread.Sleep(1);

            }

            System.Threading.Thread.MemoryBarrier();

            Phase3_Types_Of_Dark_Water_III currentType = Phase3_Types_Of_Dark_Water_III.NONE;

            switch (phase3_roundOfDarkWaterIii)
            {

                case 1:
                    {

                        currentType = Phase3_Types_Of_Dark_Water_III.SHORT;

                        break;

                    }

                case 2:
                    {

                        currentType = Phase3_Types_Of_Dark_Water_III.MEDIUM;

                        break;

                    }

                case 3:
                    {

                        currentType = Phase3_Types_Of_Dark_Water_III.LONG;

                        break;

                    }

                default:
                    {
                        // Just a placeholder and should never be reached.

                        return;

                    }

            }

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Double_Group_åŒåˆ†ç»„æ³•)
            {

                if (phase3_numberOfDarkWaterIiiHasBeenProcessed == 6)
                {

                    int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                    bool goLeft = phase3_doubleGroup_shouldGoLeft(myIndex);
                    bool stayInTheGroup = phase3_doubleGroup_shouldStayInTheGroup(myIndex);

                    for (int i = 0; i < 8; ++i)
                    {

                        if (phase3_typeOfDarkWaterIii[i] == currentType)
                        {

                            currentProperty = accessory.Data.GetDefaultDrawProperties();

                            currentProperty.Name = "Phase3_Range_Of_Dark_Water_III";
                            currentProperty.Scale = new(6);
                            currentProperty.Owner = accessory.Data.PartyList[i];
                            currentProperty.DestoryAt = 5000;

                            if (phase3_roundOfDarkWaterIii == 1 || phase3_roundOfDarkWaterIii == 3)
                            {

                                if (phase3_doubleGroup_shouldGoLeft(i) == goLeft)
                                {

                                    currentProperty.Color = accessory.Data.DefaultSafeColor;

                                }

                                else
                                {

                                    currentProperty.Color = accessory.Data.DefaultDangerColor;

                                }

                            }

                            if (phase3_roundOfDarkWaterIii == 2)
                            {

                                bool endUpWithTheLeftGroup = true;
                                int doubleGroupIndexOfMyMedium = 0;

                                if (0 <= myIndex && myIndex <= 3)
                                {

                                    endUpWithTheLeftGroup = true;

                                }

                                if (4 <= myIndex && myIndex <= 7)
                                {

                                    endUpWithTheLeftGroup = false;

                                }

                                if (!stayInTheGroup)
                                {

                                    endUpWithTheLeftGroup = (!endUpWithTheLeftGroup);

                                }

                                if (endUpWithTheLeftGroup)
                                {

                                    for (doubleGroupIndexOfMyMedium = 0;
                                        phase3_typeOfDarkWaterIii[phase3_doubleGroup_priority_asAConstant[doubleGroupIndexOfMyMedium]] != Phase3_Types_Of_Dark_Water_III.MEDIUM
                                        &&
                                        doubleGroupIndexOfMyMedium < 8;
                                        ++doubleGroupIndexOfMyMedium) ;

                                }

                                else
                                {

                                    for (doubleGroupIndexOfMyMedium = 7;
                                        phase3_typeOfDarkWaterIii[phase3_doubleGroup_priority_asAConstant[doubleGroupIndexOfMyMedium]] != Phase3_Types_Of_Dark_Water_III.MEDIUM
                                        &&
                                        doubleGroupIndexOfMyMedium >= 0;
                                        --doubleGroupIndexOfMyMedium) ;

                                }

                                if (doubleGroupIndexOfMyMedium < 0 || doubleGroupIndexOfMyMedium > 7)
                                {

                                    currentProperty.Color = accessory.Data.DefaultDangerColor;

                                }

                                else
                                {

                                    if (phase3_doubleGroup_priority_asAConstant[doubleGroupIndexOfMyMedium] == i)
                                    {

                                        currentProperty.Color = accessory.Data.DefaultSafeColor;

                                    }

                                    else
                                    {

                                        currentProperty.Color = accessory.Data.DefaultDangerColor;

                                    }

                                }

                            }

                            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);

                        }

                    }

                    return;

                }

            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.High_Priority_As_Locomotives_è½¦å¤´ä½Žæ¢æ³•_MMW)
            {

                if (phase3_numberOfDarkWaterIiiHasBeenProcessed == 6)
                {

                    bool goLeft = phase3_locomotive_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));

                    for (int i = 0; i < 8; ++i)
                    {

                        if (phase3_typeOfDarkWaterIii[i] == currentType)
                        {

                            currentProperty = accessory.Data.GetDefaultDrawProperties();

                            currentProperty.Name = "Phase3_Range_Of_Dark_Water_III";
                            currentProperty.Scale = new(6);
                            currentProperty.Owner = accessory.Data.PartyList[i];
                            currentProperty.DestoryAt = 5000;

                            if (phase3_locomotive_shouldGoLeft(i) == goLeft)
                            {

                                currentProperty.Color = accessory.Data.DefaultSafeColor;

                            }

                            else
                            {

                                currentProperty.Color = accessory.Data.DefaultDangerColor;

                            }

                            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);

                        }

                    }

                    return;

                }

            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Moglin_Meow_Or_Baby_Wheelchair_Based_On_Signs_æ ¹æ®ç›®æ ‡æ ‡è®°çš„èŽ«çµå–µæ³•æˆ–å®å®æ¤…æ³•)
            {

                if (phase3_numberOfMarksHaveBeenRecorded >= 8)
                {

                    bool goLeft = phase3_moglinMeow_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));

                    for (int i = 0; i < 8; ++i)
                    {

                        if (phase3_typeOfDarkWaterIii[i] == currentType)
                        {

                            currentProperty = accessory.Data.GetDefaultDrawProperties();

                            currentProperty.Name = "Phase3_Range_Of_Dark_Water_III";
                            currentProperty.Scale = new(6);
                            currentProperty.Owner = accessory.Data.PartyList[i];
                            currentProperty.DestoryAt = 5000;

                            if (phase3_moglinMeow_shouldGoLeft(i) == goLeft)
                            {

                                currentProperty.Color = accessory.Data.DefaultSafeColor;

                            }

                            else
                            {

                                currentProperty.Color = accessory.Data.DefaultDangerColor;

                            }

                            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);

                        }

                    }

                    return;

                }

            }

            for (int i = 0; i < 8; ++i)
            {

                if (phase3_typeOfDarkWaterIii[i] == currentType)
                {

                    currentProperty = accessory.Data.GetDefaultDrawProperties();

                    currentProperty.Name = "Phase3_Range_Of_Dark_Water_III";
                    currentProperty.Scale = new(6);
                    currentProperty.Owner = accessory.Data.PartyList[i];
                    currentProperty.Color = accessory.Data.DefaultDangerColor;
                    currentProperty.DestoryAt = 5000;

                    accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);

                }

            }

            if (Enable_Text_Prompts)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    accessory.Method.TextInfo("åˆ†æ‘Š", 2000);

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    accessory.Method.TextInfo("Stack", 2000);

                }

            }

            if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    accessory.TTS("åˆ†æ‘Š", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    accessory.TTS("Stack", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

            }

        }

        [ScriptMethod(name: "Phase3 Guidance Of Dark Water III",
            eventType: EventTypeEnum.StatusRemove,
            eventCondition: ["StatusID:2458"],
            suppress: 2000)]

        public void Phase3_Guidance_Of_Dark_Water_III_é»‘æš—ç‹‚æ°´æŒ‡è·¯(Event @event, ScriptAccessory accessory)
        {
            
            if(@event["SourceId"].Equals("00000000")) {

                return;

            }
            
            System.Threading.Thread.MemoryBarrier();

            while (System.Threading.Interlocked.CompareExchange(ref phase3_guidanceSemaphoreOfDarkWaterIii, 0, 1) == 0)
            {

                System.Threading.Thread.Sleep(1);

            }

            System.Threading.Thread.MemoryBarrier();

            if (phase3_numberOfDarkWaterIiiHasBeenProcessed != 6)
            {

                return;

            }

            bool targetPositionConfirmed = false;
            string prompt = "";
            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase3_Guidance_Of_Dark_Water_III";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.DestoryAt = 5000;

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Double_Group_åŒåˆ†ç»„æ³•)
            {

                bool goLeft = phase3_doubleGroup_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));
                bool stayInTheGroup = phase3_doubleGroup_shouldStayInTheGroup(accessory.Data.PartyList.IndexOf(accessory.Data.Me));

                if (Enable_Developer_Mode)
                {

                    accessory.Method.SendChat($"""
                                               /e 
                                               goLeft={goLeft}
                                               stayInTheGroup={stayInTheGroup}
                                               phase3_roundOfDarkWaterIii={phase3_roundOfDarkWaterIii}
                                               
                                               """);

                }

                switch (phase3_roundOfDarkWaterIii)
                {

                    case 1:
                        {

                            currentProperty.TargetPosition = (goLeft) ? (new Vector3(93, 0, 100)) : (new Vector3(107, 0, 100));

                            targetPositionConfirmed = true;

                            if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                            {

                                prompt = (goLeft) ? ("å·¦ä¾§åˆ†æ‘Š") : ("å³ä¾§åˆ†æ‘Š");

                            }

                            if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                            {

                                prompt = (goLeft) ? ("Stack on the left") : ("Stack on the right");

                            }

                            break;

                        }

                    case 2:
                        {

                            if (stayInTheGroup)
                            {

                                if (0 <= accessory.Data.PartyList.IndexOf(accessory.Data.Me)
                                   &&
                                   accessory.Data.PartyList.IndexOf(accessory.Data.Me) <= 3)
                                {

                                    currentProperty.TargetPosition = phase3_doubleGroup_leftPositionToStackOfTheSecondRound;

                                    targetPositionConfirmed = true;

                                }

                                if (4 <= accessory.Data.PartyList.IndexOf(accessory.Data.Me)
                                   &&
                                   accessory.Data.PartyList.IndexOf(accessory.Data.Me) <= 7)
                                {

                                    currentProperty.TargetPosition = phase3_doubleGroup_rightPositionToStackOfTheSecondRound;

                                    targetPositionConfirmed = true;

                                }

                            }

                            else
                            {

                                if (0 <= accessory.Data.PartyList.IndexOf(accessory.Data.Me)
                                   &&
                                   accessory.Data.PartyList.IndexOf(accessory.Data.Me) <= 3)
                                {

                                    currentProperty.TargetPosition = phase3_doubleGroup_rightPositionToStackOfTheSecondRound;

                                    targetPositionConfirmed = true;

                                }

                                if (4 <= accessory.Data.PartyList.IndexOf(accessory.Data.Me)
                                   &&
                                   accessory.Data.PartyList.IndexOf(accessory.Data.Me) <= 7)
                                {

                                    currentProperty.TargetPosition = phase3_doubleGroup_leftPositionToStackOfTheSecondRound;

                                    targetPositionConfirmed = true;

                                }

                            }

                            if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                            {

                                prompt = (stayInTheGroup) ? ("æœ¬ç»„åˆ†æ‘Š") : ("å¯¹ç»„åˆ†æ‘Š");

                            }

                            if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                            {

                                prompt = (stayInTheGroup) ? ("Stack in the current group") : ("Stack in the opposite group");

                            }

                            break;

                        }

                    case 3:
                        {

                            if (ParseObjectId(phase3_bossId, out var bossId))
                            {

                                var bossObject = accessory.Data.Objects.SearchById(bossId);

                                if (bossObject != null)
                                {

                                    float currentRotation = bossObject.Rotation;
                                    currentRotation = -(currentRotation - float.Pi);

                                    Vector3 groupPosition = new Vector3(100, 0, 100);

                                    if (Enable_Developer_Mode)
                                    {

                                        accessory.Method.SendChat($"""
                                                               /e 
                                                               currentRotation={currentRotation}

                                                               """);

                                    }

                                    if (goLeft)
                                    {

                                        groupPosition = new Vector3(bossObject.Position.X - 6.89f,
                                                                  bossObject.Position.Y,
                                                                  bossObject.Position.Z + 6.89f);

                                    }

                                    else
                                    {

                                        groupPosition = new Vector3(bossObject.Position.X + 6.89f,
                                                                  bossObject.Position.Y,
                                                                  bossObject.Position.Z + 6.89f);

                                    }

                                    groupPosition = RotatePoint(groupPosition, bossObject.Position, currentRotation);

                                    currentProperty.TargetPosition = groupPosition;

                                    targetPositionConfirmed = true;

                                }

                            }

                            if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                            {

                                prompt = (goLeft) ? ("å·¦ä¾§åˆ†æ‘Š") : ("å³ä¾§åˆ†æ‘Š");

                            }

                            if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                            {

                                prompt = (goLeft) ? ("Stack on the left") : ("Stack on the right");

                            }

                            break;

                        }

                    default:
                        {
                            // Just a placeholder and should never be reached.

                            break;

                        }

                }

            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.High_Priority_As_Locomotives_è½¦å¤´ä½Žæ¢æ³•_MMW)
            {

                bool goLeft = phase3_locomotive_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));

                if (Enable_Developer_Mode)
                {

                    accessory.Method.SendChat($"""
                                               /e 
                                               goLeft={goLeft}
                                               phase3_roundOfDarkWaterIii={phase3_roundOfDarkWaterIii}
                                               
                                               """);

                }

                switch (phase3_roundOfDarkWaterIii)
                {

                    case 1:
                        {

                            currentProperty.TargetPosition = (goLeft) ? (new Vector3(93, 0, 100)) : (new Vector3(107, 0, 100));

                            targetPositionConfirmed = true;

                            if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                            {

                                prompt = (goLeft) ? ("å·¦ä¾§åˆ†æ‘Š") : ("å³ä¾§åˆ†æ‘Š");

                            }

                            if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                            {

                                prompt = (goLeft) ? ("Stack on the left") : ("Stack on the right");

                            }

                            break;

                        }

                    case 2:
                        {

                            currentProperty.TargetPosition = (goLeft) ?
                                (phase3_locomotive_leftPositionToStackOfTheSecondRound) :
                                (phase3_locomotive_rightPositionToStackOfTheSecondRound);

                            targetPositionConfirmed = true;

                            if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                            {

                                prompt = "åœºä¸­å½“å‰ä¾§åˆ†æ‘Š";

                            }

                            if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                            {

                                prompt = "Stack on this side of the center";

                            }

                            break;

                        }

                    case 3:
                        {

                            if (ParseObjectId(phase3_bossId, out var bossId))
                            {

                                var bossObject = accessory.Data.Objects.SearchById(bossId);

                                if (bossObject != null)
                                {

                                    float currentRotation = bossObject.Rotation;
                                    currentRotation = -(currentRotation - float.Pi);

                                    Vector3 groupPosition = new Vector3(100, 0, 100);

                                    if (Enable_Developer_Mode)
                                    {

                                        accessory.Method.SendChat($"""
                                                               /e 
                                                               currentRotation={currentRotation}

                                                               """);

                                    }

                                    if (goLeft)
                                    {

                                        groupPosition = new Vector3(bossObject.Position.X - 6.89f,
                                                                  bossObject.Position.Y,
                                                                  bossObject.Position.Z + 6.89f);

                                    }

                                    else
                                    {

                                        groupPosition = new Vector3(bossObject.Position.X + 6.89f,
                                                                  bossObject.Position.Y,
                                                                  bossObject.Position.Z + 6.89f);

                                    }

                                    groupPosition = RotatePoint(groupPosition, bossObject.Position, currentRotation);

                                    currentProperty.TargetPosition = groupPosition;

                                    targetPositionConfirmed = true;

                                }

                            }

                            if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                            {

                                prompt = (goLeft) ? ("å·¦ä¾§åˆ†æ‘Š") : ("å³ä¾§åˆ†æ‘Š");

                            }

                            if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                            {

                                prompt = (goLeft) ? ("Stack on the left") : ("Stack on the right");

                            }

                            break;

                        }

                    default:
                        {
                            // Just a placeholder and should never be reached.

                            break;

                        }

                }

            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Moglin_Meow_Or_Baby_Wheelchair_Based_On_Signs_æ ¹æ®ç›®æ ‡æ ‡è®°çš„èŽ«çµå–µæ³•æˆ–å®å®æ¤…æ³•)
            {

                bool goLeft = phase3_moglinMeow_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));

                if (Enable_Developer_Mode)
                {

                    accessory.Method.SendChat($"""
                                               /e 
                                               goLeft={goLeft}
                                               phase3_roundOfDarkWaterIii={phase3_roundOfDarkWaterIii}
                                               
                                               """);

                }

                switch (phase3_roundOfDarkWaterIii)
                {

                    case 1:
                        {

                            currentProperty.TargetPosition = (goLeft) ? (new Vector3(93, 0, 100)) : (new Vector3(107, 0, 100));

                            targetPositionConfirmed = true;

                            if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                            {

                                prompt = (goLeft) ? ("å·¦ä¾§åˆ†æ‘Š") : ("å³ä¾§åˆ†æ‘Š");

                            }

                            if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                            {

                                prompt = (goLeft) ? ("Stack on the left") : ("Stack on the right");

                            }

                            break;

                        }

                    case 2:
                        {

                            currentProperty.TargetPosition = (goLeft) ?
                                (phase3_moglinMeow_leftPositionToStackOfTheSecondRound) :
                                (phase3_moglinMeow_rightPositionToStackOfTheSecondRound);

                            targetPositionConfirmed = true;

                            if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                            {

                                prompt = "åœºä¸­å½“å‰ä¾§åˆ†æ‘Š";

                            }

                            if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                            {

                                prompt = "Stack on this side of the center";

                            }

                            break;

                        }

                    case 3:
                        {

                            if (ParseObjectId(phase3_bossId, out var bossId))
                            {

                                var bossObject = accessory.Data.Objects.SearchById(bossId);

                                if (bossObject != null)
                                {

                                    float currentRotation = bossObject.Rotation;
                                    currentRotation = -(currentRotation - float.Pi);

                                    Vector3 groupPosition = new Vector3(100, 0, 100);

                                    if (Enable_Developer_Mode)
                                    {

                                        accessory.Method.SendChat($"""
                                                               /e 
                                                               currentRotation={currentRotation}

                                                               """);

                                    }

                                    if (goLeft)
                                    {

                                        groupPosition = new Vector3(bossObject.Position.X - 6.89f,
                                                                  bossObject.Position.Y,
                                                                  bossObject.Position.Z + 6.89f);

                                    }

                                    else
                                    {

                                        groupPosition = new Vector3(bossObject.Position.X + 6.89f,
                                                                  bossObject.Position.Y,
                                                                  bossObject.Position.Z + 6.89f);

                                    }

                                    groupPosition = RotatePoint(groupPosition, bossObject.Position, currentRotation);

                                    currentProperty.TargetPosition = groupPosition;

                                    targetPositionConfirmed = true;

                                }

                            }

                            if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                            {

                                prompt = (goLeft) ? ("å·¦ä¾§åˆ†æ‘Š") : ("å³ä¾§åˆ†æ‘Š");

                            }

                            if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                            {

                                prompt = (goLeft) ? ("Stack on the left") : ("Stack on the right");

                            }

                            break;

                        }

                    default:
                        {
                            // Just a placeholder and should never be reached.

                            break;

                        }

                }

            }

            if (targetPositionConfirmed)
            {

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            }

            if (!prompt.Equals(""))
            {

                if (Enable_Text_Prompts)
                {

                    accessory.Method.TextInfo(prompt, 2500);

                }

                accessory.TTS($"{prompt}", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

            }

        }

        private bool phase3_doubleGroup_shouldStayInTheGroup(int currentIndex)
        {

            bool inTheLeftGroup = true;

            if (0 <= currentIndex && currentIndex <= 3)
            {

                inTheLeftGroup = true;

            }

            if (4 <= currentIndex && currentIndex <= 7)
            {

                inTheLeftGroup = false;

            }

            if (inTheLeftGroup == phase3_doubleGroup_shouldGoLeft(currentIndex))
            {

                return true;

            }

            else
            {

                return false;

            }

        }

        private bool phase3_doubleGroup_shouldGoLeft(int currentIndex)
        {

            if (currentIndex < 0 || currentIndex > 7)
            {

                return true;

            }

            int doubleGroupIndex = phase3_doubleGroup_getDoubleGroupIndex(currentIndex);
            Phase3_Types_Of_Dark_Water_III currentType = phase3_typeOfDarkWaterIii[currentIndex];
            bool goLeft = true;

            for (int i = 0; i < 8; ++i)
            {

                if (phase3_typeOfDarkWaterIii[phase3_doubleGroup_priority_asAConstant[i]] == currentType && i != doubleGroupIndex)
                {

                    if (i > doubleGroupIndex)
                    {

                        goLeft = true;
                        // Should go left.

                        break;

                    }

                    if (i < doubleGroupIndex)
                    {

                        goLeft = false;
                        // Should go right.

                        break;

                    }

                }

            }

            return goLeft;

        }

        private int phase3_doubleGroup_getDoubleGroupIndex(int currentIndex)
        {

            for (int i = 0; i < 8; ++i)
            {

                if (currentIndex == phase3_doubleGroup_priority_asAConstant[i])
                {

                    return i;

                }

            }

            return currentIndex;
            // Just a placeholder and should never be reached.

        }

        private bool phase3_locomotive_shouldGoLeft(int currentIndex)
        {

            if (currentIndex < 0 || currentIndex > 7)
            {

                return true;

            }

            int locomotiveIndex = phase3_locomotive_getLocomotiveIndex(currentIndex);
            Phase3_Types_Of_Dark_Water_III currentType = phase3_typeOfDarkWaterIii[currentIndex];
            bool goLeft = true;

            for (int i = 0; i < 8; ++i)
            {

                if (phase3_typeOfDarkWaterIii[phase3_locomotive_priority_asAConstant[i]] == currentType && i != locomotiveIndex)
                {

                    if (i > locomotiveIndex)
                    {

                        goLeft = true;
                        // Should go left.

                        break;

                    }

                    if (i < locomotiveIndex)
                    {

                        goLeft = false;
                        // Should go right.

                        break;

                    }

                }

            }

            return goLeft;

        }

        private int phase3_locomotive_getLocomotiveIndex(int currentIndex)
        {

            for (int i = 0; i < 8; ++i)
            {

                if (currentIndex == phase3_locomotive_priority_asAConstant[i])
                {

                    return i;

                }

            }

            return currentIndex;
            // Just a placeholder and should never be reached.

        }

        private bool phase3_moglinMeow_shouldGoLeft(int currentIndex)
        {

            if (currentIndex < 0 || currentIndex > 7)
            {

                return true;

            }

            if (phase3_marksOfPlayers[currentIndex] == MarkType.Attack1
               ||
               phase3_marksOfPlayers[currentIndex] == MarkType.Attack2
               ||
               phase3_marksOfPlayers[currentIndex] == MarkType.Attack3
               ||
               phase3_marksOfPlayers[currentIndex] == MarkType.Attack4)
            {

                return true;

            }

            if (phase3_marksOfPlayers[currentIndex] == MarkType.Bind1
               ||
               phase3_marksOfPlayers[currentIndex] == MarkType.Bind2
               ||
               phase3_marksOfPlayers[currentIndex] == MarkType.Bind3
               ||
               phase3_marksOfPlayers[currentIndex] == MarkType.Square)
            {

                return false;

            }

            return true;
            // Just a placeholder and should never be reached.

        }

        [ScriptMethod(name: "Phase3 Range Of Spirit Taker",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40288"])]

        public void Phase3_Range_Of_Spirit_Taker_ç¢Žçµä¸€å‡»èŒƒå›´(Event @event, ScriptAccessory accessory)
        {

            if (parse!=32)
            {

                return;

            }

            for (int i = 0; i < 8; ++i)
            {

                var currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase3_Range_Of_Spirit_Taker";
                currentProperty.Scale = new(5);
                currentProperty.Owner = accessory.Data.PartyList[i];
                currentProperty.Color = accessory.Data.DefaultDangerColor;
                currentProperty.Delay = 1250;
                currentProperty.DestoryAt = 2500;

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);

            }

            System.Threading.Thread.Sleep(1000);

            if (Enable_Text_Prompts)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    accessory.Method.TextInfo("åˆ†æ•£", 2000);

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    accessory.Method.TextInfo("Spread", 2000);

                }

            }

            if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    accessory.TTS("åˆ†æ•£", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    accessory.TTS("Spread", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

            }

        }

        [ScriptMethod(name: "Phase3 Guidance Of Spirit Taker",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40288"])]

        public void Phase3_Guidance_Of_Spirit_Taker_ç¢Žçµä¸€å‡»æŒ‡è·¯(Event @event, ScriptAccessory accessory)
        {

            if (parse!=32)
            {

                return;

            }

            bool targetPositionConfirmed = false;
            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase3_Guidance_Of_Spirit_Taker";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.Delay = 1250;
            currentProperty.DestoryAt = 2500;

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Double_Group_åŒåˆ†ç»„æ³•)
            {

                int myDoubleGroupIndex = phase3_doubleGroup_getDoubleGroupIndex(accessory.Data.PartyList.IndexOf(accessory.Data.Me));

                switch (myDoubleGroupIndex)
                {

                    case 0:
                        {
                            // H1

                            currentProperty.TargetPosition = new Vector3(85, 0, 100);
                            targetPositionConfirmed = true;

                            break;

                        }

                    case 1:
                        {
                            // H2

                            bool goLeft = phase3_doubleGroup_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));

                            if (Enable_Developer_Mode)
                            {

                                accessory.Method.SendChat($"""
                                                       /e 
                                                       goLeft={goLeft}

                                                       """);

                            }

                            currentProperty.TargetPosition = (goLeft) ? (new Vector3(93, 0, 92)) : (new Vector3(107, 0, 92));
                            targetPositionConfirmed = true;

                            break;

                        }

                    case 2:
                        {
                            // MT

                            currentProperty.TargetPosition = new Vector3(100, 0, 92);
                            targetPositionConfirmed = true;

                            break;

                        }

                    case 3:
                        {
                            // OT or ST

                            currentProperty.TargetPosition = new Vector3(100, 0, 100);
                            targetPositionConfirmed = true;

                            break;

                        }

                    case 4:
                        {
                            // M1 or D1

                            bool goLeft = phase3_doubleGroup_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));

                            if (Enable_Developer_Mode)
                            {

                                accessory.Method.SendChat($"""
                                                       /e 
                                                       goLeft={goLeft}

                                                       """);

                            }

                            currentProperty.TargetPosition = (goLeft) ? (new Vector3(93, 0, 100)) : (new Vector3(107, 0, 100));
                            targetPositionConfirmed = true;

                            break;

                        }

                    case 5:
                        {
                            // M2 or D2

                            currentProperty.TargetPosition = new Vector3(100, 0, 108);
                            targetPositionConfirmed = true;

                            break;

                        }

                    case 6:
                        {
                            // R1 or D3

                            bool goLeft = phase3_doubleGroup_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));

                            if (Enable_Developer_Mode)
                            {

                                accessory.Method.SendChat($"""
                                                       /e 
                                                       goLeft={goLeft}

                                                       """);

                            }

                            currentProperty.TargetPosition = (goLeft) ? (new Vector3(93, 0, 108)) : (new Vector3(107, 0, 108));
                            targetPositionConfirmed = true;

                            break;

                        }

                    case 7:
                        {
                            // R2 or D4

                            currentProperty.TargetPosition = new Vector3(115, 0, 100);
                            targetPositionConfirmed = true;

                            break;

                        }

                    default:
                        {
                            // Just a placeholder and should never be reached.

                            break;

                        }

                }

            }

            else
            {

                var temporaryProperty = accessory.Data.GetDefaultDrawProperties();

                Vector3 point1 = new Vector3(93f, 0f, 101f);
                Vector3 point1Extension = new Vector3(93f, 0f, 109f);
                Vector3 point2 = new Vector3(93f, 0f, 99f);
                Vector3 point2Extension = new Vector3(93f, 0f, 91f);
                Vector3 point3 = new Vector3(92f, 0f, 101f);
                Vector3 point3Extension = new Vector3(85.072f, 0f, 105f);
                Vector3 point4 = new Vector3(92f, 0f, 99f);
                Vector3 point4Extension = new Vector3(85.072f, 0f, 95f);
                Vector3 point5 = new Vector3(107f, 0f, 101f);
                Vector3 point5Extension = new Vector3(107f, 0f, 109f);
                Vector3 point6 = new Vector3(107f, 0f, 99f);
                Vector3 point6Extension = new Vector3(107f, 0f, 91f);
                Vector3 point7 = new Vector3(108f, 0f, 101f);
                Vector3 point7Extension = new Vector3(114.928f, 0f, 105f);
                Vector3 point8 = new Vector3(108f, 0f, 99f);
                Vector3 point8Extension = new Vector3(114.928f, 0f, 95f);

                temporaryProperty = accessory.Data.GetDefaultDrawProperties();

                temporaryProperty.Name = "Phase3_Rough_Guidance_Of_Spirit_Taker";
                temporaryProperty.Scale = new(2);
                temporaryProperty.Position = point1;
                temporaryProperty.TargetPosition = point1Extension;
                temporaryProperty.ScaleMode |= ScaleMode.YByDistance;
                temporaryProperty.Color = Phase3_Colour_Of_Rough_Guidance.V4.WithW(1f);
                temporaryProperty.Delay = 1250;
                temporaryProperty.DestoryAt = 2500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, temporaryProperty);

                temporaryProperty = accessory.Data.GetDefaultDrawProperties();

                temporaryProperty.Name = "Phase3_Rough_Guidance_Of_Spirit_Taker";
                temporaryProperty.Scale = new(2);
                temporaryProperty.Position = point2;
                temporaryProperty.TargetPosition = point2Extension;
                temporaryProperty.ScaleMode |= ScaleMode.YByDistance;
                temporaryProperty.Color = Phase3_Colour_Of_Rough_Guidance.V4.WithW(1f);
                temporaryProperty.Delay = 1250;
                temporaryProperty.DestoryAt = 2500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, temporaryProperty);

                temporaryProperty = accessory.Data.GetDefaultDrawProperties();

                temporaryProperty.Name = "Phase3_Rough_Guidance_Of_Spirit_Taker";
                temporaryProperty.Scale = new(2);
                temporaryProperty.Position = point3;
                temporaryProperty.TargetPosition = point3Extension;
                temporaryProperty.ScaleMode |= ScaleMode.YByDistance;
                temporaryProperty.Color = Phase3_Colour_Of_Rough_Guidance.V4.WithW(1f);
                temporaryProperty.Delay = 1250;
                temporaryProperty.DestoryAt = 2500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, temporaryProperty);

                temporaryProperty = accessory.Data.GetDefaultDrawProperties();

                temporaryProperty.Name = "Phase3_Rough_Guidance_Of_Spirit_Taker";
                temporaryProperty.Scale = new(2);
                temporaryProperty.Position = point4;
                temporaryProperty.TargetPosition = point4Extension;
                temporaryProperty.ScaleMode |= ScaleMode.YByDistance;
                temporaryProperty.Color = Phase3_Colour_Of_Rough_Guidance.V4.WithW(1f);
                temporaryProperty.Delay = 1250;
                temporaryProperty.DestoryAt = 2500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, temporaryProperty);

                temporaryProperty = accessory.Data.GetDefaultDrawProperties();

                temporaryProperty.Name = "Phase3_Rough_Guidance_Of_Spirit_Taker";
                temporaryProperty.Scale = new(2);
                temporaryProperty.Position = point5;
                temporaryProperty.TargetPosition = point5Extension;
                temporaryProperty.ScaleMode |= ScaleMode.YByDistance;
                temporaryProperty.Color = Phase3_Colour_Of_Rough_Guidance.V4.WithW(1f);
                temporaryProperty.Delay = 1250;
                temporaryProperty.DestoryAt = 2500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, temporaryProperty);

                temporaryProperty = accessory.Data.GetDefaultDrawProperties();

                temporaryProperty.Name = "Phase3_Rough_Guidance_Of_Spirit_Taker";
                temporaryProperty.Scale = new(2);
                temporaryProperty.Position = point6;
                temporaryProperty.TargetPosition = point6Extension;
                temporaryProperty.ScaleMode |= ScaleMode.YByDistance;
                temporaryProperty.Color = Phase3_Colour_Of_Rough_Guidance.V4.WithW(1f);
                temporaryProperty.Delay = 1250;
                temporaryProperty.DestoryAt = 2500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, temporaryProperty);

                temporaryProperty = accessory.Data.GetDefaultDrawProperties();

                temporaryProperty.Name = "Phase3_Rough_Guidance_Of_Spirit_Taker";
                temporaryProperty.Scale = new(2);
                temporaryProperty.Position = point7;
                temporaryProperty.TargetPosition = point7Extension;
                temporaryProperty.ScaleMode |= ScaleMode.YByDistance;
                temporaryProperty.Color = Phase3_Colour_Of_Rough_Guidance.V4.WithW(1f);
                temporaryProperty.Delay = 1250;
                temporaryProperty.DestoryAt = 2500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, temporaryProperty);

                temporaryProperty = accessory.Data.GetDefaultDrawProperties();

                temporaryProperty.Name = "Phase3_Rough_Guidance_Of_Spirit_Taker";
                temporaryProperty.Scale = new(2);
                temporaryProperty.Position = point8;
                temporaryProperty.TargetPosition = point8Extension;
                temporaryProperty.ScaleMode |= ScaleMode.YByDistance;
                temporaryProperty.Color = Phase3_Colour_Of_Rough_Guidance.V4.WithW(1f);
                temporaryProperty.Delay = 1250;
                temporaryProperty.DestoryAt = 2500;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, temporaryProperty);

            }

            if (targetPositionConfirmed)
            {

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            }

        }

        [ScriptMethod(name: "Phase3 Determine Initial Safe Positions Of Apocalypse",
            eventType: EventTypeEnum.ObjectEffect,
            eventCondition: ["Id1:4", "Id2:regex:^(16|64)$"],
            userControl: false,
            suppress: 2000)]

        public void Phase3_Determine_Initial_Safe_Positions_Of_Apocalypse_ç¡®å®šå¯ç¤ºåˆå§‹å®‰å…¨ä½ç½®(Event @event, ScriptAccessory accessory)
        {

            if (parse!=32)
            {

                return;

            }

            if (phase3_hasConfirmedInitialSafePositions)
            {

                return;

            }

            Vector3 position1OfTheSecond = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            Vector3 position2OfTheSecond = RotatePoint(position1OfTheSecond, new Vector3(100, 0, 100), float.Pi);
            int clockwise = (@event["Id2"].Equals("64")) ? (-1) : (1);
            Vector3 position1OfTheLast = RotatePoint(position1OfTheSecond, new Vector3(100, 0, 100), float.Pi / 4 * 3 * clockwise);
            Vector3 position2OfTheLast = RotatePoint(position1OfTheSecond, new Vector3(100, 0, 100), float.Pi / 4 * 3 * clockwise + float.Pi);
            Vector3 position1OfThePenultimate = RotatePoint(position1OfTheSecond, new Vector3(100, 0, 100), float.Pi / 2 * clockwise);
            Vector3 position2OfThePenultimate = RotatePoint(position1OfTheSecond, new Vector3(100, 0, 100), float.Pi / 2 * clockwise + float.Pi);
            int direction1OfTheLast = PositionTo8Dir(position1OfTheLast, new Vector3(100, 0, 100));
            int direction1OfThePenultimate = PositionTo8Dir(position1OfThePenultimate, new Vector3(100, 0, 100));
            int direction1OfTheSecond = PositionTo8Dir(position1OfTheSecond, new Vector3(100, 0, 100));

            if (Enable_Developer_Mode)
            {

                accessory.Method.SendChat($"""
                                           /e 
                                           position1OfTheLast={position1OfTheLast}
                                           position2OfTheLast={position2OfTheLast}
                                           clockwise={clockwise}
                                           position1OfThePenultimate={position1OfThePenultimate}
                                           position2OfThePenultimate={position2OfThePenultimate}
                                           position1OfTheSecond={position1OfTheSecond}
                                           position2OfTheSecond={position2OfTheSecond}
                                           direction1OfTheLast={direction1OfTheLast}
                                           direction1OfThePenultimate={direction1OfThePenultimate}
                                           direction1OfTheSecond={direction1OfTheSecond}
                                           
                                           """);

            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Double_Group_åŒåˆ†ç»„æ³•)
            {

                if (Phase3_Branch_Of_The_Double_Group_Strat == Phase3_Branches_Of_The_Double_Group_Strat.Based_On_Safe_Positions_å®‰å…¨åŒºä¸ºåŸºå‡†_MMW)
                {

                    if (Phase3_Division_Of_The_Zone == Phase3_Divisions_Of_The_Zone.North_To_Southwest_For_The_Left_Group_å·¦ç»„ä»Žæ­£åŒ—åˆ°è¥¿å—_èŽ«çµå–µä¸ŽMMW)
                    {

                        if (direction1OfTheLast == 0
                           ||
                           direction1OfTheLast == 7
                           ||
                           direction1OfTheLast == 6
                           ||
                           direction1OfTheLast == 5)
                        {

                            phase3_doubleGroup_initialSafePositionOfTheLeftGroup = position1OfTheLast;
                            phase3_doubleGroup_leftPositionToStackOfTheSecondRound = new Vector3((position1OfTheLast.X - 100) / 3 + 100,
                                                                                               position1OfTheLast.Y,
                                                                                               (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3_doubleGroup_initialSafePositionOfTheRightGroup = position2OfTheLast;
                            phase3_doubleGroup_rightPositionToStackOfTheSecondRound = new Vector3((position2OfTheLast.X - 100) / 3 + 100,
                                                                                                position2OfTheLast.Y,
                                                                                                (position2OfTheLast.Z - 100) / 3 + 100);

                            phase3_hasConfirmedInitialSafePositions = true;

                        }

                        if (direction1OfTheLast == 1
                           ||
                           direction1OfTheLast == 2
                           ||
                           direction1OfTheLast == 3
                           ||
                           direction1OfTheLast == 4)
                        {

                            phase3_doubleGroup_initialSafePositionOfTheLeftGroup = position2OfTheLast;
                            phase3_doubleGroup_leftPositionToStackOfTheSecondRound = new Vector3((position2OfTheLast.X - 100) / 3 + 100,
                                                                                               position2OfTheLast.Y,
                                                                                               (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3_doubleGroup_initialSafePositionOfTheRightGroup = position1OfTheLast;
                            phase3_doubleGroup_rightPositionToStackOfTheSecondRound = new Vector3((position1OfTheLast.X - 100) / 3 + 100,
                                                                                                position1OfTheLast.Y,
                                                                                                (position1OfTheLast.Z - 100) / 3 + 100);

                            phase3_hasConfirmedInitialSafePositions = true;

                        }

                    }

                    if (Phase3_Division_Of_The_Zone == Phase3_Divisions_Of_The_Zone.Northwest_To_South_For_The_Left_Group_å·¦ç»„ä»Žè¥¿åŒ—åˆ°æ­£å—)
                    {

                        if (direction1OfTheLast == 7
                           ||
                           direction1OfTheLast == 6
                           ||
                           direction1OfTheLast == 5
                           ||
                           direction1OfTheLast == 4)
                        {

                            phase3_doubleGroup_initialSafePositionOfTheLeftGroup = position1OfTheLast;
                            phase3_doubleGroup_leftPositionToStackOfTheSecondRound = new Vector3((position1OfTheLast.X - 100) / 3 + 100,
                                                                                               position1OfTheLast.Y,
                                                                                               (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3_doubleGroup_initialSafePositionOfTheRightGroup = position2OfTheLast;
                            phase3_doubleGroup_rightPositionToStackOfTheSecondRound = new Vector3((position2OfTheLast.X - 100) / 3 + 100,
                                                                                                position2OfTheLast.Y,
                                                                                                (position2OfTheLast.Z - 100) / 3 + 100);

                            phase3_hasConfirmedInitialSafePositions = true;

                        }

                        if (direction1OfTheLast == 0
                           ||
                           direction1OfTheLast == 1
                           ||
                           direction1OfTheLast == 2
                           ||
                           direction1OfTheLast == 3)
                        {

                            phase3_doubleGroup_initialSafePositionOfTheLeftGroup = position2OfTheLast;
                            phase3_doubleGroup_leftPositionToStackOfTheSecondRound = new Vector3((position2OfTheLast.X - 100) / 3 + 100,
                                                                                               position2OfTheLast.Y,
                                                                                               (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3_doubleGroup_initialSafePositionOfTheRightGroup = position1OfTheLast;
                            phase3_doubleGroup_rightPositionToStackOfTheSecondRound = new Vector3((position1OfTheLast.X - 100) / 3 + 100,
                                                                                                position1OfTheLast.Y,
                                                                                                (position1OfTheLast.Z - 100) / 3 + 100);

                            phase3_hasConfirmedInitialSafePositions = true;

                        }

                    }

                }

                if (Phase3_Branch_Of_The_Double_Group_Strat == Phase3_Branches_Of_The_Double_Group_Strat.Based_On_The_Second_Apocalypse_ç¬¬äºŒæ¬¡å¯ç¤ºä¸ºåŸºå‡†)
                {

                    if (Phase3_Division_Of_The_Zone == Phase3_Divisions_Of_The_Zone.North_To_Southwest_For_The_Left_Group_å·¦ç»„ä»Žæ­£åŒ—åˆ°è¥¿å—_èŽ«çµå–µä¸ŽMMW)
                    {

                        if (direction1OfTheSecond == 0
                           ||
                           direction1OfTheSecond == 7
                           ||
                           direction1OfTheSecond == 6
                           ||
                           direction1OfTheSecond == 5)
                        {

                            phase3_doubleGroup_initialSafePositionOfTheLeftGroup = position2OfTheLast;
                            phase3_doubleGroup_leftPositionToStackOfTheSecondRound = new Vector3((position2OfTheLast.X - 100) / 3 + 100,
                                                                                               position2OfTheLast.Y,
                                                                                               (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3_doubleGroup_initialSafePositionOfTheRightGroup = position1OfTheLast;
                            phase3_doubleGroup_rightPositionToStackOfTheSecondRound = new Vector3((position1OfTheLast.X - 100) / 3 + 100,
                                                                                                position1OfTheLast.Y,
                                                                                                (position1OfTheLast.Z - 100) / 3 + 100);

                            phase3_hasConfirmedInitialSafePositions = true;

                        }

                        if (direction1OfTheSecond == 1
                           ||
                           direction1OfTheSecond == 2
                           ||
                           direction1OfTheSecond == 3
                           ||
                           direction1OfTheSecond == 4)
                        {

                            phase3_doubleGroup_initialSafePositionOfTheLeftGroup = position1OfTheLast;
                            phase3_doubleGroup_leftPositionToStackOfTheSecondRound = new Vector3((position1OfTheLast.X - 100) / 3 + 100,
                                                                                               position1OfTheLast.Y,
                                                                                               (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3_doubleGroup_initialSafePositionOfTheRightGroup = position2OfTheLast;
                            phase3_doubleGroup_rightPositionToStackOfTheSecondRound = new Vector3((position2OfTheLast.X - 100) / 3 + 100,
                                                                                                position2OfTheLast.Y,
                                                                                                (position2OfTheLast.Z - 100) / 3 + 100);

                            phase3_hasConfirmedInitialSafePositions = true;

                        }

                    }

                    if (Phase3_Division_Of_The_Zone == Phase3_Divisions_Of_The_Zone.Northwest_To_South_For_The_Left_Group_å·¦ç»„ä»Žè¥¿åŒ—åˆ°æ­£å—)
                    {

                        if (direction1OfTheSecond == 7
                           ||
                           direction1OfTheSecond == 6
                           ||
                           direction1OfTheSecond == 5
                           ||
                           direction1OfTheSecond == 4)
                        {

                            phase3_doubleGroup_initialSafePositionOfTheLeftGroup = position2OfTheLast;
                            phase3_doubleGroup_leftPositionToStackOfTheSecondRound = new Vector3((position2OfTheLast.X - 100) / 3 + 100,
                                                                                               position2OfTheLast.Y,
                                                                                               (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3_doubleGroup_initialSafePositionOfTheRightGroup = position1OfTheLast;
                            phase3_doubleGroup_rightPositionToStackOfTheSecondRound = new Vector3((position1OfTheLast.X - 100) / 3 + 100,
                                                                                                position1OfTheLast.Y,
                                                                                                (position1OfTheLast.Z - 100) / 3 + 100);

                            phase3_hasConfirmedInitialSafePositions = true;

                        }

                        if (direction1OfTheSecond == 0
                           ||
                           direction1OfTheSecond == 1
                           ||
                           direction1OfTheSecond == 2
                           ||
                           direction1OfTheSecond == 3)
                        {

                            phase3_doubleGroup_initialSafePositionOfTheLeftGroup = position1OfTheLast;
                            phase3_doubleGroup_leftPositionToStackOfTheSecondRound = new Vector3((position1OfTheLast.X - 100) / 3 + 100,
                                                                                               position1OfTheLast.Y,
                                                                                               (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3_doubleGroup_initialSafePositionOfTheRightGroup = position2OfTheLast;
                            phase3_doubleGroup_rightPositionToStackOfTheSecondRound = new Vector3((position2OfTheLast.X - 100) / 3 + 100,
                                                                                                position2OfTheLast.Y,
                                                                                                (position2OfTheLast.Z - 100) / 3 + 100);

                            phase3_hasConfirmedInitialSafePositions = true;

                        }

                    }

                }

            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.High_Priority_As_Locomotives_è½¦å¤´ä½Žæ¢æ³•_MMW)
            {

                if (Phase3_Branch_Of_The_Locomotive_Strat == Phase3_Branches_Of_The_Locomotive_Strat.MT_And_M1_As_Locomotives_MTå’ŒD1ä¸ºè½¦å¤´_MMW)
                {

                    if (Phase3_Division_Of_The_Zone == Phase3_Divisions_Of_The_Zone.North_To_Southwest_For_The_Left_Group_å·¦ç»„ä»Žæ­£åŒ—åˆ°è¥¿å—_èŽ«çµå–µä¸ŽMMW)
                    {

                        if (direction1OfThePenultimate == 0
                           ||
                           direction1OfThePenultimate == 7
                           ||
                           direction1OfThePenultimate == 6
                           ||
                           direction1OfThePenultimate == 5)
                        {

                            phase3_locomotive_initialSafePositionOfTheLeftGroup = position1OfThePenultimate;
                            phase3_locomotive_leftPositionToStackOfTheSecondRound = new Vector3((position1OfTheLast.X - 100) / 3 + 100,
                                                                                              position1OfTheLast.Y,
                                                                                              (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3_locomotive_initialSafePositionOfTheRightGroup = position2OfThePenultimate;
                            phase3_locomotive_rightPositionToStackOfTheSecondRound = new Vector3((position2OfTheLast.X - 100) / 3 + 100,
                                                                                               position2OfTheLast.Y,
                                                                                               (position2OfTheLast.Z - 100) / 3 + 100);

                            phase3_hasConfirmedInitialSafePositions = true;

                        }

                        if (direction1OfThePenultimate == 1
                            ||
                            direction1OfThePenultimate == 2
                            ||
                            direction1OfThePenultimate == 3
                            ||
                            direction1OfThePenultimate == 4)
                        {

                            phase3_locomotive_initialSafePositionOfTheLeftGroup = position2OfThePenultimate;
                            phase3_locomotive_leftPositionToStackOfTheSecondRound = new Vector3((position2OfTheLast.X - 100) / 3 + 100,
                                                                                              position2OfTheLast.Y,
                                                                                              (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3_locomotive_initialSafePositionOfTheRightGroup = position1OfThePenultimate;
                            phase3_locomotive_rightPositionToStackOfTheSecondRound = new Vector3((position1OfTheLast.X - 100) / 3 + 100,
                                                                                               position1OfTheLast.Y,
                                                                                               (position1OfTheLast.Z - 100) / 3 + 100);

                            phase3_hasConfirmedInitialSafePositions = true;

                        }

                    }

                    if (Phase3_Division_Of_The_Zone == Phase3_Divisions_Of_The_Zone.Northwest_To_South_For_The_Left_Group_å·¦ç»„ä»Žè¥¿åŒ—åˆ°æ­£å—)
                    {

                        if (direction1OfThePenultimate == 7
                            ||
                            direction1OfThePenultimate == 6
                            ||
                            direction1OfThePenultimate == 5
                            ||
                            direction1OfThePenultimate == 4)
                        {

                            phase3_locomotive_initialSafePositionOfTheLeftGroup = position1OfThePenultimate;
                            phase3_locomotive_leftPositionToStackOfTheSecondRound = new Vector3((position1OfTheLast.X - 100) / 3 + 100,
                                                                                              position1OfTheLast.Y,
                                                                                              (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3_locomotive_initialSafePositionOfTheRightGroup = position2OfThePenultimate;
                            phase3_locomotive_rightPositionToStackOfTheSecondRound = new Vector3((position2OfTheLast.X - 100) / 3 + 100,
                                                                                               position2OfTheLast.Y,
                                                                                               (position2OfTheLast.Z - 100) / 3 + 100);

                            phase3_hasConfirmedInitialSafePositions = true;

                        }

                        if (direction1OfThePenultimate == 0
                            ||
                            direction1OfThePenultimate == 1
                            ||
                            direction1OfThePenultimate == 2
                            ||
                            direction1OfThePenultimate == 3)
                        {

                            phase3_locomotive_initialSafePositionOfTheLeftGroup = position2OfThePenultimate;
                            phase3_locomotive_leftPositionToStackOfTheSecondRound = new Vector3((position2OfTheLast.X - 100) / 3 + 100,
                                                                                              position2OfTheLast.Y,
                                                                                              (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3_locomotive_initialSafePositionOfTheRightGroup = position1OfThePenultimate;
                            phase3_locomotive_rightPositionToStackOfTheSecondRound = new Vector3((position1OfTheLast.X - 100) / 3 + 100,
                                                                                               position1OfTheLast.Y,
                                                                                               (position1OfTheLast.Z - 100) / 3 + 100);

                            phase3_hasConfirmedInitialSafePositions = true;

                        }

                    }

                }

                if (Phase3_Branch_Of_The_Locomotive_Strat == Phase3_Branches_Of_The_Locomotive_Strat.Others_As_Locomotives_Chinese_PF_å›½æœé‡Žé˜Ÿäººç¾¤ä¸ºè½¦å¤´)
                {

                    if (Phase3_Division_Of_The_Zone == Phase3_Divisions_Of_The_Zone.North_To_Southwest_For_The_Left_Group_å·¦ç»„ä»Žæ­£åŒ—åˆ°è¥¿å—_èŽ«çµå–µä¸ŽMMW)
                    {

                        if (direction1OfTheLast == 0
                           ||
                           direction1OfTheLast == 7
                           ||
                           direction1OfTheLast == 6
                           ||
                           direction1OfTheLast == 5)
                        {

                            phase3_locomotive_initialSafePositionOfTheLeftGroup = position1OfTheLast;
                            phase3_locomotive_leftPositionToStackOfTheSecondRound = new Vector3((position1OfTheLast.X - 100) / 3 + 100,
                                                                                              position1OfTheLast.Y,
                                                                                              (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3_locomotive_initialSafePositionOfTheRightGroup = position2OfTheLast;
                            phase3_locomotive_rightPositionToStackOfTheSecondRound = new Vector3((position2OfTheLast.X - 100) / 3 + 100,
                                                                                               position2OfTheLast.Y,
                                                                                               (position2OfTheLast.Z - 100) / 3 + 100);

                            phase3_hasConfirmedInitialSafePositions = true;

                        }

                        if (direction1OfTheLast == 1
                           ||
                           direction1OfTheLast == 2
                           ||
                           direction1OfTheLast == 3
                           ||
                           direction1OfTheLast == 4)
                        {

                            phase3_locomotive_initialSafePositionOfTheLeftGroup = position2OfTheLast;
                            phase3_locomotive_leftPositionToStackOfTheSecondRound = new Vector3((position2OfTheLast.X - 100) / 3 + 100,
                                                                                              position2OfTheLast.Y,
                                                                                              (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3_locomotive_initialSafePositionOfTheRightGroup = position1OfTheLast;
                            phase3_locomotive_rightPositionToStackOfTheSecondRound = new Vector3((position1OfTheLast.X - 100) / 3 + 100,
                                                                                               position1OfTheLast.Y,
                                                                                               (position1OfTheLast.Z - 100) / 3 + 100);

                            phase3_hasConfirmedInitialSafePositions = true;

                        }

                    }

                    if (Phase3_Division_Of_The_Zone == Phase3_Divisions_Of_The_Zone.Northwest_To_South_For_The_Left_Group_å·¦ç»„ä»Žè¥¿åŒ—åˆ°æ­£å—)
                    {

                        if (direction1OfTheLast == 7
                           ||
                           direction1OfTheLast == 6
                           ||
                           direction1OfTheLast == 5
                           ||
                           direction1OfTheLast == 4)
                        {

                            phase3_locomotive_initialSafePositionOfTheLeftGroup = position1OfTheLast;
                            phase3_locomotive_leftPositionToStackOfTheSecondRound = new Vector3((position1OfTheLast.X - 100) / 3 + 100,
                                                                                              position1OfTheLast.Y,
                                                                                              (position1OfTheLast.Z - 100) / 3 + 100);
                            phase3_locomotive_initialSafePositionOfTheRightGroup = position2OfTheLast;
                            phase3_locomotive_rightPositionToStackOfTheSecondRound = new Vector3((position2OfTheLast.X - 100) / 3 + 100,
                                                                                               position2OfTheLast.Y,
                                                                                               (position2OfTheLast.Z - 100) / 3 + 100);

                            phase3_hasConfirmedInitialSafePositions = true;

                        }

                        if (direction1OfTheLast == 0
                           ||
                           direction1OfTheLast == 1
                           ||
                           direction1OfTheLast == 2
                           ||
                           direction1OfTheLast == 3)
                        {

                            phase3_locomotive_initialSafePositionOfTheLeftGroup = position2OfTheLast;
                            phase3_locomotive_leftPositionToStackOfTheSecondRound = new Vector3((position2OfTheLast.X - 100) / 3 + 100,
                                                                                              position2OfTheLast.Y,
                                                                                              (position2OfTheLast.Z - 100) / 3 + 100);
                            phase3_locomotive_initialSafePositionOfTheRightGroup = position1OfTheLast;
                            phase3_locomotive_rightPositionToStackOfTheSecondRound = new Vector3((position1OfTheLast.X - 100) / 3 + 100,
                                                                                               position1OfTheLast.Y,
                                                                                               (position1OfTheLast.Z - 100) / 3 + 100);

                            phase3_hasConfirmedInitialSafePositions = true;

                        }

                    }

                }

            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Moglin_Meow_Or_Baby_Wheelchair_Based_On_Signs_æ ¹æ®ç›®æ ‡æ ‡è®°çš„èŽ«çµå–µæ³•æˆ–å®å®æ¤…æ³•)
            {

                if (direction1OfTheLast == 0
                   ||
                   direction1OfTheLast == 7
                   ||
                   direction1OfTheLast == 6
                   ||
                   direction1OfTheLast == 5)
                {

                    phase3_moglinMeow_initialSafePositionOfTheLeftGroup = position1OfTheLast;
                    phase3_moglinMeow_leftPositionToStackOfTheSecondRound = new Vector3((position1OfTheLast.X - 100) / 3 + 100,
                                                                                               position1OfTheLast.Y,
                                                                                               (position1OfTheLast.Z - 100) / 3 + 100);
                    phase3_moglinMeow_initialSafePositionOfTheRightGroup = position2OfTheLast;
                    phase3_moglinMeow_rightPositionToStackOfTheSecondRound = new Vector3((position2OfTheLast.X - 100) / 3 + 100,
                                                                                                position2OfTheLast.Y,
                                                                                                (position2OfTheLast.Z - 100) / 3 + 100);

                    phase3_hasConfirmedInitialSafePositions = true;

                }

                if (direction1OfTheLast == 1
                   ||
                   direction1OfTheLast == 2
                   ||
                   direction1OfTheLast == 3
                   ||
                   direction1OfTheLast == 4)
                {

                    phase3_moglinMeow_initialSafePositionOfTheLeftGroup = position2OfTheLast;
                    phase3_moglinMeow_leftPositionToStackOfTheSecondRound = new Vector3((position2OfTheLast.X - 100) / 3 + 100,
                                                                                               position2OfTheLast.Y,
                                                                                               (position2OfTheLast.Z - 100) / 3 + 100);
                    phase3_moglinMeow_initialSafePositionOfTheRightGroup = position1OfTheLast;
                    phase3_moglinMeow_rightPositionToStackOfTheSecondRound = new Vector3((position1OfTheLast.X - 100) / 3 + 100,
                                                                                                position1OfTheLast.Y,
                                                                                                (position1OfTheLast.Z - 100) / 3 + 100);

                    phase3_hasConfirmedInitialSafePositions = true;

                }

            }

        }

        [ScriptMethod(name: "P3_DelayedEchoes_Apocalypse", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:4", "Id2:regex:^(16|64)$"], suppress: 2000)]
        public void P3_DelayedEchoes_Apocalypse(Event @event, ScriptAccessory accessory)
        {
            if (parse!=32) return;
            if (P3FloorFireDone) return;
            P3FloorFireDone = true;
            Vector3 centre = new(100, 0, 100);
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            var clockwise = @event["Id2"] == "64" ? -1 : 1;
            var preTime = 100;
            //interval 11 2 2 2 2 2

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedEchoes_Apocalypse_Center";
            dp.Scale = new(9);
            dp.Position = centre;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 9700;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedEchoes_Apocalypse_StartPoint_11";
            dp.Scale = new(9);
            dp.Position = pos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 12000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedEchoes_Apocalypse_StartPoint_12";
            dp.Scale = new(9);
            dp.Position = pos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 17000 - preTime;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedEchoes_Apocalypse_StartPoint_21";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 12000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedEchoes_Apocalypse_StartPoint_22";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 17000 - preTime;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedEchoes_Apocalypse_SecondPoint_11";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 4 * clockwise);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 14000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedEchoes_Apocalypse_SecondPoint_12";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 4 * clockwise);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 19000 - preTime;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedEchoes_Apocalypse_SecondPoint_21";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 4 * clockwise + float.Pi);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 3000;
            dp.DestoryAt = 14000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedEchoes_Apocalypse_SecondPoint_22";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 4 * clockwise + float.Pi);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 19000 - preTime;
            dp.DestoryAt = 2000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedEchoes_Apocalypse_ThirdPoint_11";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 2 * clockwise);
            dp.Color = Phase3_Colour_Of_The_Penultimate_Apocalypse.V4.WithW(1f);
            dp.Delay = 3000;
            dp.DestoryAt = 8000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedEchoes_Apocalypse_ThirdPoint_12";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 2 * clockwise);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 11000 - preTime;
            dp.DestoryAt = 8000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedEchoes_Apocalypse_ThirdPoint_21";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 2 * clockwise + float.Pi);
            dp.Color = Phase3_Colour_Of_The_Penultimate_Apocalypse.V4.WithW(1f);
            dp.Delay = 3000;
            dp.DestoryAt = 8000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedEchoes_Apocalypse_ThirdPoint_22";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 2 * clockwise + float.Pi);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 11000 - preTime;
            dp.DestoryAt = 8000 - preTime;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedEchoes_Apocalypse_FourthPoint_11";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 4 * 3 * clockwise);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 15000 - preTime;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedEchoes_Apocalypse_FourthPoint_21";
            dp.Scale = new(9);
            dp.Position = RotatePoint(pos, centre, float.Pi / 4 * 3 * clockwise + float.Pi);
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 15000 - preTime;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

        }

        [ScriptMethod(name: "Phase3 Rough Guidance Of Initial Safe Positions",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40289"])]

        public void Phase3_Rough_Guidance_Of_Initial_Safe_Positions_åˆå§‹å®‰å…¨ä½ç½®ç²—ç•¥æŒ‡è·¯(Event @event, ScriptAccessory accessory)
        {

            if (parse!=32)
            {

                return;

            }

            bool targetPositionConfirmed = false;
            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase3_Rough_Guidance_Of_Initial_Safe_Positions";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.Color = Phase3_Colour_Of_Rough_Guidance.V4.WithW(1f);
            currentProperty.Delay = 500;
            currentProperty.DestoryAt = 6500;

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Double_Group_åŒåˆ†ç»„æ³•)
            {

                if (0 <= accessory.Data.PartyList.IndexOf(accessory.Data.Me)
                   &&
                   accessory.Data.PartyList.IndexOf(accessory.Data.Me) <= 3)
                {

                    currentProperty.TargetPosition = phase3_doubleGroup_initialSafePositionOfTheLeftGroup;

                    targetPositionConfirmed = true;

                }

                if (4 <= accessory.Data.PartyList.IndexOf(accessory.Data.Me)
                   &&
                   accessory.Data.PartyList.IndexOf(accessory.Data.Me) <= 7)
                {

                    currentProperty.TargetPosition = phase3_doubleGroup_initialSafePositionOfTheRightGroup;

                    targetPositionConfirmed = true;

                }

            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.High_Priority_As_Locomotives_è½¦å¤´ä½Žæ¢æ³•_MMW)
            {

                bool goLeft = phase3_locomotive_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));

                if (goLeft)
                {

                    currentProperty.TargetPosition = phase3_locomotive_initialSafePositionOfTheLeftGroup;

                    targetPositionConfirmed = true;

                }

                else
                {

                    currentProperty.TargetPosition = phase3_locomotive_initialSafePositionOfTheRightGroup;

                    targetPositionConfirmed = true;

                }

            }

            if (Phase3_Strat_Of_The_Second_Half == Phase3_Strats_Of_The_Second_Half.Moglin_Meow_Or_Baby_Wheelchair_Based_On_Signs_æ ¹æ®ç›®æ ‡æ ‡è®°çš„èŽ«çµå–µæ³•æˆ–å®å®æ¤…æ³•)
            {

                bool goLeft = phase3_moglinMeow_shouldGoLeft(accessory.Data.PartyList.IndexOf(accessory.Data.Me));

                if (goLeft)
                {

                    currentProperty.TargetPosition = phase3_moglinMeow_initialSafePositionOfTheLeftGroup;

                    targetPositionConfirmed = true;

                }

                else
                {

                    currentProperty.TargetPosition = phase3_moglinMeow_initialSafePositionOfTheRightGroup;

                    targetPositionConfirmed = true;

                }

            }

            if (targetPositionConfirmed)
            {

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            }

        }

        [ScriptMethod(name: "Phase3 Range Of Darkest Dance",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40181"])]

        public void Phase3_Range_Of_Darkest_Dance_æš—å¤œèˆžè¹ˆèŒƒå›´(Event @event, ScriptAccessory accessory)
        {

            if (parse!=32)
            {

                return;

            }

            if (!ParseObjectId(@event["SourceId"], out var sourceId))
            {

                return;

            }

            bool goBait = false;

            if (Phase3_Tank_Who_Baits_Darkest_Dance == Tanks.MT
               &&
               accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 0)
            {

                goBait = true;

            }

            if (Phase3_Tank_Who_Baits_Darkest_Dance == Tanks.OT_ST
               &&
               accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 1)
            {

                goBait = true;

            }

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase3_Range_Of_Darkest_Dance";
            currentProperty.Scale = new(8);
            currentProperty.Owner = sourceId;
            currentProperty.CentreResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;
            currentProperty.Color = Phase3_Colour_Of_Darkest_Dance.V4.WithW(3f);
            currentProperty.Delay = 2200;
            currentProperty.DestoryAt = 4000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);

            System.Threading.Thread.Sleep(2200);

            if (goBait)
            {

                if (Enable_Text_Prompts)
                {

                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                    {

                        accessory.Method.TextInfo("æœ€è¿œæ­»åˆ‘", 1500);

                    }

                    if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                    {

                        accessory.Method.TextInfo("Stay away and bait", 1500);

                    }

                }

                if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
                {

                    if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                    {

                        accessory.TTS("æœ€è¿œæ­»åˆ‘", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                    }

                    if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                    {

                        accessory.TTS("Stay away and bait", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                    }

                }

            }

            else
            {

                if (Phase3_Tank_Who_Baits_Darkest_Dance == Tanks.MT)
                {

                    if (Enable_Text_Prompts)
                    {

                        if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                        {

                            accessory.Method.TextInfo("è¿œç¦»MT", 1500);

                        }

                        if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                        {

                            accessory.Method.TextInfo("Stay away from MT", 1500);

                        }

                    }

                    if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
                    {

                        if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                        {

                            accessory.TTS("è¿œç¦»MT", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                        }

                        if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                        {

                            accessory.TTS("Stay away from MT", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                        }

                    }

                }

                if (Phase3_Tank_Who_Baits_Darkest_Dance == Tanks.OT_ST)
                {

                    if (Enable_Text_Prompts)
                    {

                        if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                        {

                            accessory.Method.TextInfo("è¿œç¦»ST", 1500);

                        }

                        if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                        {

                            accessory.Method.TextInfo("Stay away from OT", 1500);

                        }

                    }

                    if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
                    {

                        if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                        {

                            accessory.TTS("è¿œç¦»ST", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                        }

                        if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                        {

                            accessory.TTS("Stay away from OT", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                        }

                    }

                }

            }

        }

        [ScriptMethod(name: "Phase3 Guidance Of Darkest Dance",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40181"])]

        public void Phase3_Guidance_Of_Darkest_Dance_æš—å¤œèˆžè¹ˆæŒ‡è·¯(Event @event, ScriptAccessory accessory)
        {

            if (parse!=32)
            {

                return;

            }

            var tankWhoBaitsDarkestDance = accessory.Data.Objects.SearchById(accessory.Data.PartyList[1]);
            bool goBait = false;

            if (Phase3_Tank_Who_Baits_Darkest_Dance == Tanks.MT)
            {

                tankWhoBaitsDarkestDance = accessory.Data.Objects.SearchById(accessory.Data.PartyList[0]);

                if (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 0)
                {

                    goBait = true;

                }

            }

            if (Phase3_Tank_Who_Baits_Darkest_Dance == Tanks.OT_ST)
            {

                tankWhoBaitsDarkestDance = accessory.Data.Objects.SearchById(accessory.Data.PartyList[1]);

                if (accessory.Data.PartyList.IndexOf(accessory.Data.Me) == 1)
                {

                    goBait = true;

                }

            }

            if (tankWhoBaitsDarkestDance == null)
            {

                return;

            }

            // ----- Calculations of the position where the tank should bait -----
            // This part was directly inherited from Karlin's script.
            // The algorithm seems to be too mysterious to me, and it definitely works nice.
            // So as a result, except the position was tuned a bit towards the edge, I just keep the part as is.

            var dir8 = P3FloorFire % 10 % 4;
            Vector3 posN = new(100, 0, 86);
            var rot = dir8 switch
            {
                0 => 6,
                1 => 7,
                2 => 0,
                3 => 5
            };
            var pos1 = RotatePoint(posN, new(100, 0, 100), float.Pi / 4 * rot);
            var pos2 = RotatePoint(posN, new(100, 0, 100), float.Pi / 4 * rot + float.Pi);
            var dealpos = ((pos1 - tankWhoBaitsDarkestDance.Position).Length() < (pos2 - tankWhoBaitsDarkestDance.Position).Length()) ? (pos1) : (pos2);

            Vector3 positionToBait = new Vector3((dealpos.X - 100) / 3 * 4 + 100,
                                               dealpos.Y,
                                               (dealpos.Z - 100) / 3 * 4 + 100);

            // ----- -----

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            if (goBait)
            {

                currentProperty.Owner = accessory.Data.Me;
                currentProperty.Color = accessory.Data.DefaultSafeColor;

            }

            else
            {

                if (Phase3_Tank_Who_Baits_Darkest_Dance == Tanks.MT)
                {

                    currentProperty.Owner = accessory.Data.PartyList[0];
                    currentProperty.Color = Phase3_Colour_Of_Darkest_Dance.V4.WithW(1f);

                }

                else
                {

                    currentProperty.Owner = accessory.Data.PartyList[1];
                    currentProperty.Color = Phase3_Colour_Of_Darkest_Dance.V4.WithW(1f);

                }

            }

            currentProperty.Name = "Phase3_Guidance_Of_Darkest_Dance";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.TargetPosition = positionToBait;
            currentProperty.Delay = 2200;
            currentProperty.DestoryAt = 4000;

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

        }

        [ScriptMethod(name: "P3_DelayedEchoes_KnockbackWarning", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40182", "TargetIndex:1"])]
        public void P3_DelayedEchoes_KnockbackWarning(Event @event, ScriptAccessory accessory)
        {
            if (parse!=32) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedEchoes_KnockbackWarning1";
            dp.Scale = new(2, 21);
            dp.Owner = accessory.Data.Me;
            dp.TargetObject = sid;
            dp.Rotation = float.Pi;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P3_DelayedEchoes_KnockbackWarning2";
            dp.Scale = new(2);
            dp.Owner = sid;
            dp.TargetObject = accessory.Data.Me;
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Color = accessory.Data.DefaultDangerColor.WithW(3);
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P3_DelayedEchoes_ApocalypseRecord", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:4", "Id2:regex:^(16|64)$"], userControl: false)]
        public void P3_DelayedEchoes_ApocalypseRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse!=32) return;
            lock (this)
            {
                if (P3FloorFire != -1) return;
                Vector3 centre = new(100, 0, 100);
                var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                P3FloorFire = PositionTo8Dir(pos, new(100, 0, 100));
                P3FloorFire += @event["Id2"] == "64" ? 10 : 20;
            }

        }

        [ScriptMethod(name: "Phase3 Determine The Final Position Of The Boss",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40300"],
            userControl: false)]

        public void Phase3_Determine_The_Final_Position_Of_The_Boss_ç¡®å®šBossçš„æœ€ç»ˆä½ç½®(Event @event, ScriptAccessory accessory)
        {

            if (parse!=32)
            {

                return;

            }

            phase3_finalPositionOfTheBoss = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);

        }

        [ScriptMethod(name: "Phase3 Initial Position Of The Boss In Phase4",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40300"])]

        public void Phase3_Initial_Position_Of_The_Boss_In_Phase4_P4æ—¶Bossçš„åˆå§‹ä½ç½®(Event @event, ScriptAccessory accessory)
        {

            if (parse!=32)
            {

                return;

            }

            if (phase3_finalPositionOfTheBoss.Equals(new Vector3(100, 0, 100)))
            {

                return;

            }

            bool inTheNorth = true;

            if (phase3_finalPositionOfTheBoss.Z <= 100)
            {

                inTheNorth = false;

            }

            if (phase3_finalPositionOfTheBoss.Z >= 100)
            {

                inTheNorth = true;

            }

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase3_Initial_Position_Of_The_Boss_In_Phase4";
            currentProperty.Scale = new(7);
            currentProperty.Position = (inTheNorth) ? (new Vector3(100, 0, 90)) : (new Vector3(100, 0, 110));
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.DestoryAt = 9250;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);

            System.Threading.Thread.Sleep(2000);

            if (Enable_Text_Prompts)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    accessory.Method.TextInfo(((inTheNorth) ? ("Bosså³å°†å‡ºçŽ°åœ¨æ­£åŒ—") : ("Bosså³å°†å‡ºçŽ°åœ¨æ­£å—")), 7250);

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    accessory.Method.TextInfo(((inTheNorth) ? ("The Boss will appear in the north") : ("The Boss will appear in the south")), 7250);

                }

            }

            if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    accessory.TTS($"{((inTheNorth) ? ("Bosså³å°†å‡ºçŽ°åœ¨æ­£åŒ—") : ("Bosså³å°†å‡ºçŽ°åœ¨æ­£å—"))}",
                        Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    accessory.TTS(
                        $"{((inTheNorth) ? ("The Boss will appear in the north") : ("The Boss will appear in the south"))}",
                        Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

            }

        }

        private int MyLampIndex(int myPartyIndex)
        {
            var nLampIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                if (P3Lamp[i] == 1 && P3Lamp[(i + 3) % 8] == 1 && P3Lamp[(i + 5) % 8] == 1)
                {
                    nLampIndex = i;
                    break;
                }
            }
            if (Phase3_Strat_Of_The_First_Half == Phase3_Strats_Of_The_First_Half.Moogle_èŽ«å¤åŠ›_èŽ«çµå–µä¸ŽMMW)
            {
                //Short Fire
                if (P3FireBuff[myPartyIndex] == 1)
                {
                    if (myPartyIndex < 4)
                    {
                        return (nLampIndex + 4) % 8;
                    }
                    else
                    {
                        var lowIndex = P3FireBuff.LastIndexOf(1);
                        if (lowIndex != myPartyIndex)
                        {
                            return (nLampIndex + 7) % 8;
                        }
                        else
                        {
                            return (nLampIndex + 1) % 8;
                        }
                    }

                }
                //Medium Fire
                if (P3FireBuff[myPartyIndex] == 2)
                {
                    if (myPartyIndex < 4) return (nLampIndex + 6) % 8;
                    else return (nLampIndex + 2) % 8;
                }
                //Long Fire
                if (P3FireBuff[myPartyIndex] == 3)
                {
                    if (myPartyIndex < 4)
                    {
                        var highIndex = P3FireBuff.IndexOf(3);
                        if (highIndex == myPartyIndex)
                        {
                            return (nLampIndex + 5) % 8;
                        }
                        else
                        {
                            return (nLampIndex + 3) % 8;
                        }
                    }
                    else
                    {
                        return (nLampIndex + 0) % 8;
                    }

                }
                //Ice
                if (P3FireBuff[myPartyIndex] == 4)
                {
                    if (myPartyIndex < 4) return (nLampIndex + 4) % 8;
                    else return (nLampIndex + 0) % 8;
                }
            }

            return -1;
        }
        #endregion

        #region Phase_4

        [ScriptMethod(name: "----- Phase 4 ----- (No actual meaning for this toggle)",
            eventType: EventTypeEnum.NpcYell,
            eventCondition: ["Send these, the homeless, tempest-tost to me",
                            "é€æ¥é‚£äº›æ— å®¶å¯å½’ï¼Œè¢«é£Žå¹é›¨æ·‹çš„äºº"])]

        public void Phase4_Placeholder(Event @event, ScriptAccessory accessory) { }

        [ScriptMethod(name: "P4_Crystallize_Transition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40246"], userControl: false)]
        public void P4_Crystallize_Transition(Event @event, ScriptAccessory accessory)
        {
            parse=41;
        }
        [ScriptMethod(name: "P4_FragmentOfFate_Collect", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40174"], userControl: false)]
        public void P4_FragmentOfFate_Collect(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            P4FragmentId = sid;
        }
        [ScriptMethod(name: "P4_Crystallize_AkhRhai", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40237"])]
        public void P4_Crystallize_AkhRhai(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_Crystallize_AkhRhai";
            dp.Scale = new(4);
            dp.Owner = sid;
            dp.TargetObject = sid;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 8000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Phase4 Prompt Before Akh Rhai",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40246"])]

        public void Phase4_Prompt_Before_Akh_Rhai_å¤©å…‰è½®å›žå‰æç¤º(Event @event, ScriptAccessory accessory)
        {

            if (Enable_Text_Prompts)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    accessory.Method.TextInfo("é›†åˆå¹¶è¿œç¦»æœªæ¥çš„ç¢Žç‰‡", 9500);

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    accessory.Method.TextInfo("Get together and stay away from Fragment of Fate", 9500);

                }

            }

            if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    accessory.TTS("é›†åˆå¹¶è¿œç¦»æœªæ¥çš„ç¢Žç‰‡", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    accessory.TTS("Get together and stay away from Fragment of Fate", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

            }

        }

        [ScriptMethod(name: "Phase4 Prompt To Dodge Akh Rhai",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40186"])]

        public void Phase4_Prompt_To_Dodge_Akh_Rhai_å¤©å…‰è½®å›žèº²é¿æç¤º(Event @event, ScriptAccessory accessory)
        {

            if (Enable_Text_Prompts)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    accessory.Method.TextInfo("è·‘ï¼", 3000);

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    accessory.Method.TextInfo("Run!", 3000);

                }

            }

            if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    accessory.TTS("è·‘ï¼", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    accessory.TTS("Run!", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

            }

        }

        [ScriptMethod(name: "P4_DarklitDragonsong_Transition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40239"], userControl: false)]
        public void P4_DarklitDragonsong_Transition(Event @event, ScriptAccessory accessory)
        {
            parse=42;
            P4Tether = [-1, -1, -1, -1, -1, -1, -1, -1];
            P4Stack = [0, 0, 0, 0, 0, 0, 0, 0];
            P4TetherDone = false;
            phase4_1_ManualReset.Reset();
            phase4_1_TetherCount = 0;
        }

        [ScriptMethod(name: "Phase4 Initial Position Before Darklit Dragonsong",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40239"])]

        public void Phase4_Initial_Position_Before_Darklit_Dragonsong_æš—å…‰é¾™è¯—å‰é¢„ç«™ä½(Event @event, ScriptAccessory accessory)
        {

            if (parse!=41
               &&
               parse!=42)
            {

                return;

            }

            List<Vector3> initialPosition = [
                new Vector3(95.5f,0f,94f),
                new Vector3(98.5f,0f,94f),
                new Vector3(101.5f,0f,94f),
                new Vector3(104.5f,0f,94f),
                new Vector3(95.5f,0f,106f),
                new Vector3(98.5f,0f,106f),
                new Vector3(101.5f,0f,106f),
                new Vector3(104.5f,0f,106f),
            ];

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            for (int i = 0; i < initialPosition.Count; ++i)
            {

                currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase4_Initial_Position_Before_Darklit_Dragonsong";
                currentProperty.Scale = new(0.5f);
                currentProperty.Position = initialPosition[i];
                currentProperty.Color = (i == myIndex) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
                currentProperty.DestoryAt = 5500;

                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);

            }

            currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase4_Initial_Position_Before_Darklit_Dragonsong";
            currentProperty.Scale = new(2);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.TargetPosition = initialPosition[myIndex];
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.DestoryAt = 5500;

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

        }

        [ScriptMethod(name: "P4_DarklitDragonsong_BuffRecord", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2461"], userControl: false)]
        public void P4_DarklitDragonsong_BuffRecord(Event @event, ScriptAccessory accessory)
        {
            if (parse!=42) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var tIndex = accessory.Data.PartyList.IndexOf(((uint)tid));
            P4Stack[tIndex] = 1;
        }
        [ScriptMethod(name: "P4_DarklitDragonsong_TetherCollect", eventType: EventTypeEnum.Tether, eventCondition: ["Id:006E"], userControl: false)]
        public void P4_DarklitDragonsong_TetherCollect(Event @event, ScriptAccessory accessory)
        {
            if (parse!=42) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var sIndex = accessory.Data.PartyList.IndexOf(((uint)sid));
            var tIndex = accessory.Data.PartyList.IndexOf(((uint)tid));
            lock (this)
            {
                P4Tether[sIndex] = tIndex;
                phase4_1_TetherCount++;
                if (phase4_1_TetherCount == 4)
                {
                    phase4_1_ManualReset.Set();
                }
            }


        }
        [ScriptMethod(name: "P4_DarklitDragonsong_BaitCone", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40187"])]
        public void P4_DarklitDragonsong_BaitCone(Event @event, ScriptAccessory accessory)
        {
            if (parse!=42) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            for (uint i = 1; i < 5; i++)
            {
                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_DarklitDragonsong_BaitCone";
                dp.Scale = new(20);
                dp.Radian = float.Pi / 3;
                dp.Owner = sid;
                dp.TargetResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
                dp.TargetOrderIndex = i;
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.Delay = 4000;
                dp.DestoryAt = 5000;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);
            }
        }
        [ScriptMethod(name: "P4_DarklitDragonsong_SpiritTaker", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40187"])]
        public void P4_DarklitDragonsong_SpiritTaker(Event @event, ScriptAccessory accessory)
        {
            if (parse!=42) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_DarklitDragonsong_SpiritTaker_Crystal";
            dp.Scale = new(8.5f);
            dp.Owner = P4FragmentId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            for (int i = 0; i < 8; i++)
            {
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_DarklitDragonsong_SpiritTaker";
                dp.Scale = new(5);
                dp.Owner = accessory.Data.PartyList[i];
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 3500;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }
        }
        [ScriptMethod(name: "P4_DarklitDragonsong_HolyWings", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4022[78])$"])]
        public void P4_DarklitDragonsong_HolyWings(Event @event, ScriptAccessory accessory)
        {
            if (parse!=42) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_DarklitDragonsong_HolyWings";
            dp.Scale = new(40, 20);
            dp.Owner = sid;
            dp.Rotation = @event["ActionId"] == "40227" ? float.Pi / 2 : float.Pi / -2;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P4_DarklitDragonsong_WaterStack", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4022[78])$"])]
        public void P4_DarklitDragonsong_WaterStack(Event @event, ScriptAccessory accessory)
        {
            var tIndex = P4Tether[0] == -1 ? 1 : 0;
            var nIndex = P4Tether[2] == -1 ? 3 : 2;
            var d1Index = -1;
            var d2Index = -1;
            List<int> upGroup = [];
            List<int> downGroup = [];
            for (int i = 4; i < 7; i++)
            {
                for (int j = i + 1; j < 8; j++)
                {
                    if (P4Tether[i] != -1 && P4Tether[j] != -1)
                    {
                        d1Index = i;
                        d2Index = j;
                    }
                }
            }
            // tether highD lowD bowtie
            if ((P4Tether[tIndex] == d1Index && P4Tether[d2Index] == tIndex) || (P4Tether[tIndex] == d2Index && P4Tether[d1Index] == tIndex))
            {
                upGroup.Add(tIndex);
                upGroup.Add(nIndex);
                downGroup.Add(d1Index);
                downGroup.Add(d2Index);
            }
            // tether highD n square
            if ((P4Tether[tIndex] == d1Index && P4Tether[nIndex] == tIndex) || (P4Tether[d1Index] == tIndex && P4Tether[tIndex] == nIndex))
            {
                upGroup.Add(d1Index);
                upGroup.Add(nIndex);
                downGroup.Add(tIndex);
                downGroup.Add(d2Index);
            }
            // tether lowD n hourglass
            if ((P4Tether[tIndex] == d2Index && P4Tether[nIndex] == tIndex) || (P4Tether[d2Index] == tIndex && P4Tether[tIndex] == nIndex))
            {
                upGroup.Add(tIndex);
                upGroup.Add(d1Index);
                downGroup.Add(nIndex);
                downGroup.Add(d2Index);
            }

            var stack1 = P4Stack.IndexOf(1);
            var stack2 = P4Stack.LastIndexOf(1);
            var tetherStack = P4Tether[stack1] == -1 ? stack2 : stack1;
            var idleStack = P4Tether[stack1] == -1 ? stack1 : stack2;

            List<int> idles = [];
            for (int i = 0; i < 8; i++)
            {
                if (P4Tether[i] == -1)
                {
                    idles.Add(i);
                }
            }
            var ii = idles.IndexOf(idleStack);

            if (Phase4_Strat_Of_The_First_Half == Phase4_Strats_Of_The_First_Half.Double_Swaps_Baiting_First_å…ˆå¼•å¯¼å†åŒæ¢)
            {
                if (upGroup.Contains(tetherStack))
                {
                    //stack tether on top
                    if (ii == 0 || ii == 2)
                    {
                        downGroup.Add(idles[0]);//t
                        downGroup.Add(idles[2]);//highD
                        upGroup.Add(idles[1]);//n
                        upGroup.Add(idles[3]);//lowD
                    }
                    if (ii == 1 || ii == 3)
                    {
                        downGroup.Add(idles[1]);
                        downGroup.Add(idles[3]);
                        upGroup.Add(idles[0]);
                        upGroup.Add(idles[2]);
                    }
                }
                if (downGroup.Contains(tetherStack))
                {
                    //stack tether on bottom
                    if (ii == 0 || ii == 2)
                    {
                        upGroup.Add(idles[0]);
                        upGroup.Add(idles[2]);
                        downGroup.Add(idles[1]);
                        downGroup.Add(idles[3]);
                    }
                    if (ii == 1 || ii == 3)
                    {
                        upGroup.Add(idles[1]);
                        upGroup.Add(idles[3]);
                        downGroup.Add(idles[0]);
                        downGroup.Add(idles[2]);
                    }
                }
            }
            if (Phase4_Strat_Of_The_First_Half == Phase4_Strats_Of_The_First_Half.Single_Swap_Baiting_After_å…ˆå•æ¢å†å¼•å¯¼_èŽ«çµå–µä¸ŽMMW || Phase4_Strat_Of_The_First_Half == Phase4_Strats_Of_The_First_Half.Single_Swap_Baiting_First_å…ˆå¼•å¯¼å†å•æ¢)
            {
                if (upGroup.Contains(tetherStack))
                {
                    //stack tether on top
                    if (ii == 0)//idle t stack
                    {
                        downGroup.Add(idles[0]);//t
                        downGroup.Add(idles[3]);//lowD
                        upGroup.Add(idles[2]);//highD
                        upGroup.Add(idles[1]);//n
                    }
                    if (ii == 1)//idle n stack
                    {
                        upGroup.Add(idles[0]);//t
                        upGroup.Add(idles[3]);//lowD
                        downGroup.Add(idles[2]);//highD
                        downGroup.Add(idles[1]);//n
                    }
                    if (ii == 2 || ii == 3)//idle D stack
                    {
                        upGroup.Add(idles[0]);//t
                        downGroup.Add(idles[3]);//lowD
                        downGroup.Add(idles[2]);//highD
                        upGroup.Add(idles[1]);//n
                    }

                }
                if (downGroup.Contains(tetherStack))
                {
                    //stack tether on bottom
                    if (ii == 0 || ii == 1)//tn stack
                    {
                        upGroup.Add(idles[0]);//t
                        downGroup.Add(idles[3]);//lowD
                        downGroup.Add(idles[2]);//highD
                        upGroup.Add(idles[1]);//n
                    }
                    if (ii == 2)//highD stack
                    {
                        downGroup.Add(idles[0]);//t
                        downGroup.Add(idles[3]);//lowD
                        upGroup.Add(idles[2]);//highD
                        upGroup.Add(idles[1]);//n
                    }
                    if (ii == 3)//lowD stack
                    {
                        upGroup.Add(idles[0]);//t
                        upGroup.Add(idles[3]);//lowD
                        downGroup.Add(idles[2]);//highD
                        downGroup.Add(idles[1]);//n
                    }
                }
            }

            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_DarklitDragonsong_Stack";
            dp.Scale = new(6);
            dp.Owner = accessory.Data.PartyList[tetherStack];
            dp.Color = upGroup.Contains(tetherStack) == upGroup.Contains(myindex) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_DarklitDragonsong_Stack";
            dp.Scale = new(6);
            dp.Owner = accessory.Data.PartyList[idleStack];
            dp.Color = upGroup.Contains(idleStack) == upGroup.Contains(myindex) ? accessory.Data.DefaultSafeColor : accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_DarklitDragonsong_Stack_Crystal";
            dp.Scale = new(9.5f);
            dp.Owner = P4FragmentId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P4_DarklitDragonsong_EndlessEpiphany", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40249"])]
        public void P4_DarklitDragonsong_EndlessEpiphany(Event @event, ScriptAccessory accessory)
        {
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_DarklitDragonsong_EndlessEpiphany";
            dp.Scale = new(4);
            dp.Owner = sid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.OwnerTarget;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 6000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        #region SomberDance
        [ScriptMethod(name: "P4_DarklitDragonsong_FarBoss", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40283"])]
        public void P4_DarklitDragonsong_FarBoss(Event @event, ScriptAccessory accessory)
        {
            if (parse!=42) return;
            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_DarklitDragonsong_FarBoss";
            dp.Scale = new(8);
            dp.Owner = sid;
            dp.CentreResolvePattern = PositionResolvePatternEnum.PlayerFarestOrder;
            dp.Color = Phase4_Colour_Of_Somber_Dance.V4.WithW(3f);
            dp.Delay = 2000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);



        }
        [ScriptMethod(name: "P4_DarklitDragonsong_NearBoss", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40284"])]
        public void P4_DarklitDragonsong_NearBoss(Event @event, ScriptAccessory accessory)
        {
            if (parse!=42) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["TargetPosition"]);

            var dp2 = accessory.Data.GetDefaultDrawProperties();
            dp2.Name = "P4_DarklitDragonsong_NearBoss";
            dp2.Scale = new(8);
            dp2.Position = pos;
            dp2.CentreResolvePattern = PositionResolvePatternEnum.PlayerNearestOrder;
            dp2.Color = Phase4_Colour_Of_Somber_Dance.V4.WithW(3f);
            dp2.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp2);

        }
        #endregion
        [ScriptMethod(name: "P4_DarklitDragonsong_StayAwayFromTethered", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:40271"], suppress: 2000)]
        public void P4_DarklitDragonsong_StayAwayFromTethered(Event @event, ScriptAccessory accessory)
        {
            if (parse!=42) return;
            if (P4Tether[accessory.Data.PartyList.IndexOf(accessory.Data.Me)] == -1) return;
            if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
            {
                if (Enable_Text_Prompts) accessory.Method.TextInfo("çº¿æœªæ¶ˆå¤±,ä¿æŒè·ç¦»", 1500);
                accessory.TTS($"çº¿æœªæ¶ˆå¤±,ä¿æŒè·ç¦»",
                    Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }

            if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
            {
                if (Enable_Text_Prompts) accessory.Method.TextInfo("The tether is still, keep your distance", 1500);
                accessory.TTS($"The tether is still, keep your distance",
                    Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);
            }
        }
        [ScriptMethod(name: "P4_DarklitDragonsong_TowerPosition", eventType: EventTypeEnum.Tether, eventCondition: ["Id:006E"])]
        public void P4_DarklitDragonsong_TowerPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse!=42) return;

            if (!ParseObjectId(@event["SourceId"], out var sid)) return;
            if (sid != accessory.Data.Me) return;
            //accessory.Log.Debug("çº¿");
            
            System.Threading.Thread.MemoryBarrier();
            
            phase4_1_ManualReset.WaitOne();
            
            System.Threading.Thread.MemoryBarrier();

            var tIndex = P4Tether[0] == -1 ? 1 : 0;
            var nIndex = P4Tether[2] == -1 ? 3 : 2;
            var d1Index = -1;
            var d2Index = -1;
            List<int> upGroup = [];
            List<int> downGroup = [];
            for (int i = 4; i < 7; i++)
            {
                for (int j = i + 1; j < 8; j++)
                {
                    if (P4Tether[i] != -1 && P4Tether[j] != -1)
                    {
                        d1Index = i;
                        d2Index = j;
                    }
                }
            }
            // tether highD lowD bowtie
            if ((P4Tether[tIndex] == d1Index && P4Tether[d2Index] == tIndex) || (P4Tether[tIndex] == d2Index && P4Tether[d1Index] == tIndex))
            {
                upGroup.Add(tIndex);
                upGroup.Add(nIndex);
                downGroup.Add(d1Index);
                downGroup.Add(d2Index);
            }
            // tether highD n square
            if ((P4Tether[tIndex] == d1Index && P4Tether[nIndex] == tIndex) || (P4Tether[d1Index] == tIndex && P4Tether[tIndex] == nIndex))
            {
                upGroup.Add(d1Index);
                upGroup.Add(nIndex);
                downGroup.Add(tIndex);
                downGroup.Add(d2Index);
            }
            // tether lowD n hourglass
            if ((P4Tether[tIndex] == d2Index && P4Tether[nIndex] == tIndex) || (P4Tether[d2Index] == tIndex && P4Tether[tIndex] == nIndex))
            {
                upGroup.Add(tIndex);
                upGroup.Add(d1Index);
                downGroup.Add(nIndex);
                downGroup.Add(d2Index);
            }

            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            Vector3 dealpos = upGroup.Contains(myIndex) ? new(100, 0, 92) : new(100, 0, 108);

            var dur = 10000;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_DarklitDragonsong_TowerPosition";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_DarklitDragonsong_TowerPosition";
            dp.Scale = new(4);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Position = dealpos;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Circle, dp);


        }
        [ScriptMethod(name: "P4_DarklitDragonsong_BaitPosition", eventType: EventTypeEnum.Tether, eventCondition: ["Id:006E"], suppress: 2000)]
        public void P4_DarklitDragonsong_BaitPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse!=42) return;
            
            System.Threading.Thread.MemoryBarrier();
            
            phase4_1_ManualReset.WaitOne();
            
            System.Threading.Thread.MemoryBarrier();
            
            Vector3 dealpos = new();
            if (Phase4_Strat_Of_The_First_Half == Phase4_Strats_Of_The_First_Half.Double_Swaps_Baiting_First_å…ˆå¼•å¯¼å†åŒæ¢ || Phase4_Strat_Of_The_First_Half == Phase4_Strats_Of_The_First_Half.Single_Swap_Baiting_First_å…ˆå¼•å¯¼å†å•æ¢)
            {
                List<int> idles = [];
                for (int i = 0; i < 8; i++)
                {
                    if (P4Tether[i] == -1)
                    {
                        idles.Add(i);
                    }
                }
                var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                if (!idles.Contains(myIndex)) return;
                dealpos = idles.IndexOf(myIndex) switch
                {
                    0 => new(095.8f, 0, 098.0f),
                    1 => new(104.2f, 0, 098.0f),
                    2 => new(095.8f, 0, 102.0f),
                    3 => new(104.2f, 0, 102.0f),
                };
            }
            if (Phase4_Strat_Of_The_First_Half == Phase4_Strats_Of_The_First_Half.Single_Swap_Baiting_After_å…ˆå•æ¢å†å¼•å¯¼_èŽ«çµå–µä¸ŽMMW)
            {
                var tIndex = P4Tether[0] == -1 ? 1 : 0;
                var nIndex = P4Tether[2] == -1 ? 3 : 2;
                var d1Index = -1;
                var d2Index = -1;
                List<int> upGroup = [];
                List<int> downGroup = [];
                for (int i = 4; i < 7; i++)
                {
                    for (int j = i + 1; j < 8; j++)
                    {
                        if (P4Tether[i] != -1 && P4Tether[j] != -1)
                        {
                            d1Index = i;
                            d2Index = j;
                        }
                    }
                }
                // tether highD lowD bowtie
                if ((P4Tether[tIndex] == d1Index && P4Tether[d2Index] == tIndex) || (P4Tether[tIndex] == d2Index && P4Tether[d1Index] == tIndex))
                {
                    upGroup.Add(tIndex);
                    upGroup.Add(nIndex);
                    downGroup.Add(d1Index);
                    downGroup.Add(d2Index);
                }
                // tether highD n square
                if ((P4Tether[tIndex] == d1Index && P4Tether[nIndex] == tIndex) || (P4Tether[d1Index] == tIndex && P4Tether[tIndex] == nIndex))
                {
                    upGroup.Add(d1Index);
                    upGroup.Add(nIndex);
                    downGroup.Add(tIndex);
                    downGroup.Add(d2Index);
                }
                // tether lowD n hourglass
                if ((P4Tether[tIndex] == d2Index && P4Tether[nIndex] == tIndex) || (P4Tether[d2Index] == tIndex && P4Tether[tIndex] == nIndex))
                {
                    upGroup.Add(tIndex);
                    upGroup.Add(d1Index);
                    downGroup.Add(nIndex);
                    downGroup.Add(d2Index);
                }
                //Phase4_Strat_Of_The_First_Half
                var stack1 = P4Stack.IndexOf(1);
                var stack2 = P4Stack.LastIndexOf(1);
                var tetherStack = P4Tether[stack1] == -1 ? stack2 : stack1;
                var idleStack = P4Tether[stack1] == -1 ? stack1 : stack2;

                List<int> idles = [];
                for (int i = 0; i < 8; i++)
                {
                    if (P4Tether[i] == -1)
                    {
                        idles.Add(i);
                    }
                }
                var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
                var ii = idles.IndexOf(idleStack);
                if (upGroup.Contains(tetherStack))
                {
                    //stack tether on top
                    if (ii == 0)
                    {
                        dealpos = idles.IndexOf(myIndex) switch
                        {
                            2 => new(095.8f, 0, 098.0f),
                            1 => new(104.2f, 0, 098.0f),
                            0 => new(095.8f, 0, 102.0f),
                            3 => new(104.2f, 0, 102.0f),
                        };
                    }
                    if (ii == 1)
                    {
                        dealpos = idles.IndexOf(myIndex) switch
                        {
                            0 => new(095.8f, 0, 098.0f),
                            3 => new(104.2f, 0, 098.0f),
                            2 => new(095.8f, 0, 102.0f),
                            1 => new(104.2f, 0, 102.0f),
                        };
                    }
                    if (ii == 2 || ii == 3)
                    {
                        dealpos = idles.IndexOf(myIndex) switch
                        {
                            0 => new(095.8f, 0, 098.0f),
                            1 => new(104.2f, 0, 098.0f),
                            2 => new(095.8f, 0, 102.0f),
                            3 => new(104.2f, 0, 102.0f),
                        };
                    }

                }
                if (downGroup.Contains(tetherStack))
                {
                    //stack tether on bottom
                    if (ii == 2)
                    {
                        dealpos = idles.IndexOf(myIndex) switch
                        {
                            2 => new(095.8f, 0, 098.0f),
                            1 => new(104.2f, 0, 098.0f),
                            0 => new(095.8f, 0, 102.0f),
                            3 => new(104.2f, 0, 102.0f),
                        };
                    }
                    if (ii == 3)
                    {
                        dealpos = idles.IndexOf(myIndex) switch
                        {
                            0 => new(095.8f, 0, 098.0f),
                            3 => new(104.2f, 0, 098.0f),
                            2 => new(095.8f, 0, 102.0f),
                            1 => new(104.2f, 0, 102.0f),
                        };
                    }
                    if (ii == 0 || ii == 1)
                    {
                        dealpos = idles.IndexOf(myIndex) switch
                        {
                            0 => new(095.8f, 0, 098.0f),
                            1 => new(104.2f, 0, 098.0f),
                            2 => new(095.8f, 0, 102.0f),
                            3 => new(104.2f, 0, 102.0f),
                        };
                    }
                }
            }



            var dur = 10000;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_DarklitDragonsong_BaitPosition";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = dur;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
        }
        [ScriptMethod(name: "P4_DarklitDragonsong_StackPosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(4022[78])$"])]
        public void P4_DarklitDragonsong_StackPosition(Event @event, ScriptAccessory accessory)
        {
            var tIndex = P4Tether[0] == -1 ? 1 : 0;
            var nIndex = P4Tether[2] == -1 ? 3 : 2;
            var d1Index = -1;
            var d2Index = -1;
            List<int> upGroup = [];
            List<int> downGroup = [];
            for (int i = 4; i < 7; i++)
            {
                for (int j = i + 1; j < 8; j++)
                {
                    if (P4Tether[i] != -1 && P4Tether[j] != -1)
                    {
                        d1Index = i;
                        d2Index = j;
                    }
                }
            }
            // tether highD lowD bowtie
            if ((P4Tether[tIndex] == d1Index && P4Tether[d2Index] == tIndex) || (P4Tether[tIndex] == d2Index && P4Tether[d1Index] == tIndex))
            {
                upGroup.Add(tIndex);
                upGroup.Add(nIndex);
                downGroup.Add(d1Index);
                downGroup.Add(d2Index);
            }
            // tether highD n square
            if ((P4Tether[tIndex] == d1Index && P4Tether[nIndex] == tIndex) || (P4Tether[d1Index] == tIndex && P4Tether[tIndex] == nIndex))
            {
                upGroup.Add(d1Index);
                upGroup.Add(nIndex);
                downGroup.Add(tIndex);
                downGroup.Add(d2Index);
            }
            // tether lowD n hourglass
            if ((P4Tether[tIndex] == d2Index && P4Tether[nIndex] == tIndex) || (P4Tether[d2Index] == tIndex && P4Tether[tIndex] == nIndex))
            {
                upGroup.Add(tIndex);
                upGroup.Add(d1Index);
                downGroup.Add(nIndex);
                downGroup.Add(d2Index);
            }
            //Phase4_Strat_Of_The_First_Half
            var stack1 = P4Stack.IndexOf(1);
            var stack2 = P4Stack.LastIndexOf(1);
            var tetherStack = P4Tether[stack1] == -1 ? stack2 : stack1;
            var idleStack = P4Tether[stack1] == -1 ? stack1 : stack2;

            List<int> idles = [];
            for (int i = 0; i < 8; i++)
            {
                if (P4Tether[i] == -1)
                {
                    idles.Add(i);
                }
            }
            var ii = idles.IndexOf(idleStack);
            if (Phase4_Strat_Of_The_First_Half == Phase4_Strats_Of_The_First_Half.Double_Swaps_Baiting_First_å…ˆå¼•å¯¼å†åŒæ¢)
            {
                if (upGroup.Contains(tetherStack))
                {
                    //stack tether on top
                    if (ii == 0 || ii == 2)
                    {
                        downGroup.Add(idles[0]);//t
                        downGroup.Add(idles[2]);//highD
                        upGroup.Add(idles[1]);//n
                        upGroup.Add(idles[3]);//lowD
                    }
                    if (ii == 1 || ii == 3)
                    {
                        downGroup.Add(idles[1]);
                        downGroup.Add(idles[3]);
                        upGroup.Add(idles[0]);
                        upGroup.Add(idles[2]);
                    }
                }
                if (downGroup.Contains(tetherStack))
                {
                    //stack tether on bottom
                    if (ii == 0 || ii == 2)
                    {
                        upGroup.Add(idles[0]);
                        upGroup.Add(idles[2]);
                        downGroup.Add(idles[1]);
                        downGroup.Add(idles[3]);
                    }
                    if (ii == 1 || ii == 3)
                    {
                        upGroup.Add(idles[1]);
                        upGroup.Add(idles[3]);
                        downGroup.Add(idles[0]);
                        downGroup.Add(idles[2]);
                    }
                }
            }
            if (Phase4_Strat_Of_The_First_Half == Phase4_Strats_Of_The_First_Half.Single_Swap_Baiting_After_å…ˆå•æ¢å†å¼•å¯¼_èŽ«çµå–µä¸ŽMMW || Phase4_Strat_Of_The_First_Half == Phase4_Strats_Of_The_First_Half.Single_Swap_Baiting_First_å…ˆå¼•å¯¼å†å•æ¢)
            {
                if (upGroup.Contains(tetherStack))
                {
                    //stack tether on top
                    if (ii == 0)//idle t stack
                    {
                        downGroup.Add(idles[0]);//t
                        downGroup.Add(idles[3]);//lowD
                        upGroup.Add(idles[2]);//highD
                        upGroup.Add(idles[1]);//n
                    }
                    if (ii == 1)//idle n stack
                    {
                        upGroup.Add(idles[0]);//t
                        upGroup.Add(idles[3]);//lowD
                        downGroup.Add(idles[2]);//highD
                        downGroup.Add(idles[1]);//n
                    }
                    if (ii == 2 || ii == 3)//idle D stack
                    {
                        upGroup.Add(idles[0]);//t
                        downGroup.Add(idles[3]);//lowD
                        downGroup.Add(idles[2]);//highD
                        upGroup.Add(idles[1]);//n
                    }

                }
                if (downGroup.Contains(tetherStack))
                {
                    //stack tether on bottom
                    if (ii == 0 || ii == 1)//tn stack
                    {
                        upGroup.Add(idles[0]);//t
                        downGroup.Add(idles[3]);//lowD
                        downGroup.Add(idles[2]);//highD
                        upGroup.Add(idles[1]);//n
                    }
                    if (ii == 2)//highD stack
                    {
                        downGroup.Add(idles[0]);//t
                        downGroup.Add(idles[3]);//lowD
                        upGroup.Add(idles[2]);//highD
                        upGroup.Add(idles[1]);//n
                    }
                    if (ii == 3)//lowD stack
                    {
                        upGroup.Add(idles[0]);//t
                        upGroup.Add(idles[3]);//lowD
                        downGroup.Add(idles[2]);//highD
                        downGroup.Add(idles[1]);//n
                    }
                }
            }


            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            Vector3 dealpos = new(@event["ActionId"] == "40227" ? 105 : 95, 0, upGroup.Contains(myindex) ? 92.5f : 107.5f);

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_DarklitDragonsong_StackPosition";
            dp.Scale = new(2);
            dp.ScaleMode |= ScaleMode.YByDistance;
            dp.Owner = accessory.Data.Me;
            dp.TargetPosition = dealpos;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.DestoryAt = 5000;
            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);


        }

        public class CrystallizeTime
        {
            public ScriptAccessory Sa { get; set; } = null!;
            public PriorityDict Pr { get; set; } = null!;
            public ulong LeftWyrmSid { get; set; } = 0;
            public ulong RightWyrmSid { get; set; } = 0;
            public int LeftIcePlayerIdx { get; set; } = -1;
            public int RightIcePlayerIdx { get; set; } = -1;
            public int LeftWindPlayerIdx { get; set; } = -1;
            public int RightWindPlayerIdx { get; set; } = -1;

            public void Init(ScriptAccessory accessory, PriorityDict priorityDict)
            {
                Sa = accessory;
                Pr = priorityDict;
                LeftWyrmSid = 0;
                RightWyrmSid = 0;
                LeftIcePlayerIdx = -1;
                RightIcePlayerIdx = -1;
                LeftWindPlayerIdx = -1;
                RightWindPlayerIdx = -1;
            }
        }

        public class PriorityDict
        {
            // ReSharper disable once NullableWarningSuppressionIsUsed
            public ScriptAccessory sa { get; set; } = null!;
            // ReSharper disable once NullableWarningSuppressionIsUsed
            public Dictionary<int, int> Priorities { get; set; } = null!;
            public string Annotation { get; set; } = "";
            public int ActionCount { get; set; } = 0;

            public void Init(ScriptAccessory accessory, string annotation, int partyNum = 8)
            {
                sa = accessory;
                Priorities = new Dictionary<int, int>();
                for (var i = 0; i < partyNum; i++)
                {
                    Priorities.Add(i, 0);
                }
                Annotation = annotation;
                ActionCount = 0;
            }

            /// <summary>
            /// Add priority for a specific key
            /// </summary>
            /// <param name="idx">key</param>
            /// <param name="priority">priority value</param>
            public void AddPriority(int idx, int priority)
            {
                Priorities[idx] += priority;
            }

            /// <summary>
            /// Find the first num keys with smallest values from Priorities, return a new Dict
            /// </summary>
            /// <param name="num"></param>
            /// <returns></returns>
            public List<KeyValuePair<int, int>> SelectSmallPriorityIndices(int num)
            {
                return SelectMiddlePriorityIndices(0, num);
            }

            /// <summary>
            /// Find the first num keys with largest values from Priorities, return a new Dict
            /// </summary>
            /// <param name="num"></param>
            /// <returns></returns>
            public List<KeyValuePair<int, int>> SelectLargePriorityIndices(int num)
            {
                return SelectMiddlePriorityIndices(0, num, true);
            }

            /// <summary>
            /// Find the keys with ascending order and middle values, return a new Dict
            /// </summary>
            /// <param name="skip">skip elements. If starting from the second element, skip=1</param>
            /// <param name="num"></param>
            /// <param name="descending">descending order, default false</param>
            /// <returns></returns>
            public List<KeyValuePair<int, int>> SelectMiddlePriorityIndices(int skip, int num, bool descending = false)
            {
                if (Priorities.Count < skip + num)
                    return new List<KeyValuePair<int, int>>();

                IEnumerable<KeyValuePair<int, int>> sortedPriorities;
                if (descending)
                {
                    // Sort by value descending, then by key
                    sortedPriorities = Priorities
                        .OrderByDescending(pair => pair.Value) // sort by value first
                        .ThenBy(pair => pair.Key) // then by key
                        .Skip(skip) // skip first 'skip' elements
                        .Take(num); // take first 'num' key-value pairs
                }
                else
                {
                    // Sort by value ascending, then by key
                    sortedPriorities = Priorities
                        .OrderBy(pair => pair.Value) // sort by value first
                        .ThenBy(pair => pair.Key) // then by key
                        .Skip(skip) // skip first 'skip' elements
                        .Take(num); // take first 'num' key-value pairs
                }

                return sortedPriorities.ToList();
            }

            /// <summary>
            /// Find the key at the idx-th position in ascending order from Priorities, return
            /// </summary>
            /// <param name="idx"></param>
            /// <param name="descending">descending order, default false</param>
            /// <returns></returns>
            public KeyValuePair<int, int> SelectSpecificPriorityIndex(int idx, bool descending = false)
            {
                var sortedPriorities = SelectMiddlePriorityIndices(0, 8, descending);
                return sortedPriorities[idx];
            }

            /// <summary>
            /// Find the corresponding key from Priorities, return its sorted position
            /// </summary>
            /// <param name="key"></param>
            /// <param name="descending">descending order, default false</param>
            /// <returns></returns>
            public int FindPriorityIndexOfKey(int key, bool descending = false)
            {
                var sortedPriorities = SelectMiddlePriorityIndices(0, 8, descending);
                var i = 0;
                foreach (var dict in sortedPriorities)
                {
                    if (dict.Key == key) return i;
                    i++;
                }

                return i;
            }

            /// <summary>
            /// Add priority values at once
            /// Usually suitable for special priorities (e.g., H-T-D-H)
            /// </summary>
            /// <param name="priorities"></param>
            public void AddPriorities(List<int> priorities)
            {
                if (Priorities.Count != priorities.Count)
                    sa.Log.Error("Input list length differs from internal length");

                for (var i = 0; i < Priorities.Count; i++)
                    AddPriority(i, priorities[i]);
            }

            /// <summary>
            /// Output the priority dictionary keys and priorities
            /// </summary>
            /// <returns></returns>
            public string ShowPriorities()
            {
                var str = $"{Annotation} Priority Dictionary:\n";
                foreach (var pair in Priorities)
                {
                    str += $"Key {pair.Key} ({sa.GetPlayerJobByIndex(pair.Key)}), Value {pair.Value}\n";
                }
                sa.Log.Debug(str);
                return str;
            }

            public string PrintAnnotation()
            {
                sa.Log.Debug(Annotation);
                return Annotation;
            }

            public PriorityDict DeepCopy()
            {
                return JsonConvert.DeserializeObject<PriorityDict>(JsonConvert.SerializeObject(this)) ?? new PriorityDict();
            }

            public void AddActionCount(int count = 1)
            {
                ActionCount += count;
            }

            public bool IsActionCountEqualTo(int times)
            {
                return ActionCount == times;
            }
        }

        [ScriptMethod(name: "P4_CrystallizeTime_Transition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40240"], userControl: false)]
        public void P4_CrystallizeTime_Transition(Event @event, ScriptAccessory accessory)
        {
            parse=43;

            _pd.Init(accessory, "CrystallizeTime");
            _cry.Init(accessory, _pd);
            _events = [.. Enumerable.Range(0, 20).Select(_ => new System.Threading.ManualResetEvent(false))];

            List<int> pdList = Phase4_Priority_Of_The_Players_With_Wyrmclaw switch
            {
                // Smaller number means higher priority (more left), default is THD order.
                Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_THD_Order_æŒ‰THDé¡ºåº_èŽ«çµå–µ => [0, 1, 2, 3, 4, 5, 6, 7],
                Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_HTD_Order_æŒ‰HTDé¡ºåº_MMW => [2, 3, 0, 1, 4, 5, 6, 7],
                Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_H1TDH2_Order_æŒ‰H1TDH2é¡ºåº => [1, 2, 0, 7, 3, 4, 5, 6],
                _ => [2, 3, 0, 1, 4, 5, 6, 7],
            };
            _pd.AddPriorities(pdList);

            P4ClawBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            phase4_numberOfMajorDebuffsHaveBeenCounted = 0;
            phase4_semaphoreMajorDebuffsWereConfirmed = new System.Threading.AutoResetEvent(false);
            phase4_numberOfIncidentalDebuffsHaveBeenCounted = 0;
            phase4_semaphoreIncidentalDebuffsWereConfirmed = new System.Threading.AutoResetEvent(false);
            phase4_marksOfPlayersWithWyrmfang = [
                MarkType.Cross,
                MarkType.Cross,
                MarkType.Cross,
                MarkType.Cross,
                MarkType.Cross,
                MarkType.Cross,
                MarkType.Cross,
                MarkType.Cross
            ];
            P4OtherBuff = [0, 0, 0, 0, 0, 0, 0, 0];
            P4WaterPos = [];
            phase4_id1OfTheDrachenWanderers = "";
            phase4_id2OfTheDrachenWanderers = "";
            phase4_timesTheWyrmclawDebuffWasRemoved = 0;
            phase4_residueIdsFromEastToWest = [0, 0, 0, 0];
            phase4_guidanceOfResiduesHasBeenGenerated = false;
        }
        [ScriptMethod(name: "P4_CrystallizeTime_BuffCollect", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:regex:^(326[34]|2454|246[0123])$"], userControl: false)]
        public void P4_CrystallizeTime_BuffCollect(Event @event, ScriptAccessory accessory)
        {
            if (parse!=43) return;
            var id = @event["StatusID"];
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf(((uint)tid));
            //3623 red claw 1 short 2 long
            if (id == "3263")
            {
                if (!float.TryParse(@event["Duration"], out float dur)) return;
                P4ClawBuff[index] = dur > 20 ? 2 : 1;
                _pd.AddPriority(index, 0);      // Red +0
            }

            if (id == "3264")
            {
                P4ClawBuff[index] = 3;
                _pd.AddPriority(index, 100);    // Blue +100
            }
            //Dark 4
            if (id == "2460")
            {
                P4OtherBuff[index] = 4;
                _pd.AddPriority(index, 40);     // Dark +40
            }
            //Water 3
            if (id == "2461")
            {
                P4OtherBuff[index] = 3;
                _pd.AddPriority(index, 20);     // Water +20
            }
            //Ice 1
            if (id == "2462")
            {
                P4OtherBuff[index] = 1;
                _pd.AddPriority(index, 0);      // Ice +0
            }
            //Wind 2
            if (id == "2463")
            {
                P4OtherBuff[index] = 2;
                _pd.AddPriority(index, 10);     // Wind +10
            }
            //Earth 5
            if (id == "2454")
            {
                P4OtherBuff[index] = 5;
                _pd.AddPriority(index, 30);     // Earth +30
            }

            System.Threading.Thread.MemoryBarrier();

            if (id.Equals("3263")||id.Equals("3264"))
            {

                lock (phase4_readwriteLockOfMajorDebuffCounter_AsAConstant)
                {

                    ++phase4_numberOfMajorDebuffsHaveBeenCounted;

                    System.Threading.Thread.MemoryBarrier();

                    if (phase4_numberOfMajorDebuffsHaveBeenCounted == 8)
                    {

                        phase4_semaphoreMajorDebuffsWereConfirmed.Set();
                        _events[0].Set();   // Red/Blue recording complete
                    }

                }

            }

            if (id.Equals("2460")
               ||
               id.Equals("2461")
               ||
               id.Equals("2462")
               ||
               id.Equals("2463")
               ||
               id.Equals("2454"))
            {

                lock (phase4_readwriteLockOfIncidentalDebuffCounter_AsAConstant)
                {

                    ++phase4_numberOfIncidentalDebuffsHaveBeenCounted;

                    System.Threading.Thread.MemoryBarrier();

                    if (phase4_numberOfIncidentalDebuffsHaveBeenCounted == 8)
                    {

                        phase4_semaphoreIncidentalDebuffsWereConfirmed.Set();
                        _events[1].Set();   // Attribute recording complete
                    }

                }

            }

        }

        [ScriptMethod(name: "P4_CrystallizeTime_CalculateGroups",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40298"],
            userControl: false,
            suppress: 10000)]

        public void P4_CrystallizeTime_CalculateGroups(Event @event, ScriptAccessory accessory)
        {
            if (parse!=43) return;

            _events[0].WaitOne();
            _events[1].WaitOne();
            /*
            * In the priority value, the units digit corresponds to job priority, tens digit corresponds to attribute buff, hundreds digit corresponds to red/blue buff
            * Units digit: varies according to TDH, HTD, HTDH settings
            * Tens digit: Ice+0, Wind+10, Water+20, Earth+30, Dark+40
            * Hundreds digit: Red+0, Blue+100
            * After ascending sort, we get: [LeftRedIce, RightRedIce, LeftRedWind, RightRedWind, BlueIce, BlueWater, BlueEarth, BlueDark]
            */
            _cry.LeftIcePlayerIdx = _pd.SelectSpecificPriorityIndex(0).Key;
            _cry.RightIcePlayerIdx = _pd.SelectSpecificPriorityIndex(1).Key;
            _cry.LeftWindPlayerIdx = _pd.SelectSpecificPriorityIndex(2).Key;
            _cry.RightWindPlayerIdx = _pd.SelectSpecificPriorityIndex(3).Key;
            accessory.Log.Debug($"Recorded LeftRedIce{_cry.LeftIcePlayerIdx}, RightRedIce{_cry.RightIcePlayerIdx}, LeftRedWind{_cry.LeftWindPlayerIdx}, RightRedWind{_cry.RightWindPlayerIdx}");

            _events[2].Set();   // P4 second half priority recording complete
        }

        [ScriptMethod(name: "P4_CrystallizeTime_ReceiveExternalMarks",
            eventType: EventTypeEnum.Marker,
            eventCondition: ["Operate:Add", "Id:regex:^(0[679]|10)$"],
            userControl: false)]

        public void P4_CrystallizeTime_ReceiveExternalMarks(Event @event, ScriptAccessory accessory)
        {
            if (parse!=43) return;
            if (Phase4_Mark_Players_During_The_Second_Half) return;

            _events[2].WaitOne();
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var index = accessory.Data.PartyList.IndexOf((uint)tid);
            if (!int.TryParse(@event["Id"], out var sign)) return;

            const int stop1 = 9;
            const int stop2 = 10;
            const int bind1 = 6;
            const int bind2 = 7;

            // Obey external marks if they exist
            switch (sign)
            {
                case stop1:
                    _cry.LeftIcePlayerIdx = index;
                    accessory.Log.Debug($"CrystallizeTime: Received external stop1 mark, assigned to {index}");
                    break;
                case stop2:
                    _cry.RightIcePlayerIdx = index;
                    accessory.Log.Debug($"CrystallizeTime: Received external stop2 mark, assigned to {index}");
                    break;
                case bind1:
                    _cry.LeftWindPlayerIdx = index;
                    accessory.Log.Debug($"CrystallizeTime: Received external bind1 mark, assigned to {index}");
                    break;
                case bind2:
                    _cry.RightWindPlayerIdx = index;
                    accessory.Log.Debug($"CrystallizeTime: Received external bind2 mark, assigned to {index}");
                    break;
                default:
                    break;
            }
        }

        [ScriptMethod(name: "Phase4 Mark Teammates During The Second Half",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40298"],
            userControl: false,
            suppress: 2000)]

        public void Phase4_Mark_Teammates_During_The_Second_Half_äºŒè¿æ ‡è®°é˜Ÿå‹(Event @event, ScriptAccessory accessory)
        {

            if (parse!=43)
            {

                return;

            }

            if (!Phase4_Mark_Players_During_The_Second_Half)
            {

                return;

            }

            System.Threading.Thread.MemoryBarrier();

            phase4_semaphoreMajorDebuffsWereConfirmed.WaitOne();
            phase4_semaphoreIncidentalDebuffsWereConfirmed.WaitOne();

            System.Threading.Thread.MemoryBarrier();

            List<int> temporaryOrder=[0,1,2,3,4,5,6,7];
            string debugOutput = "";
            
            if(Phase4_Player_Type_To_Be_Marked==Phase4_Player_Types_To_Be_Marked.Both_The_Debuffs_Wyrmclaw_And_Wyrmfang_åœ£é¾™çˆªåœ£é¾™ç‰™ä¸¤ç§éƒ½æ ‡è®°
               ||
               Phase4_Player_Type_To_Be_Marked==Phase4_Player_Types_To_Be_Marked.Only_Wyrmfang_The_Blue_Debuff_ä»…åœ£é¾™ç‰™è“è‰²Debuff) {

            if (Phase4_Logic_Of_Marking_Teammates_With_Wyrmfang == Phase4_Logics_Of_Marking_Teammates_With_Wyrmfang.According_To_Debuffs_1234_From_East_To_West_æ ¹æ®Debuffä»Žä¸œåˆ°è¥¿1234
                ||
                Phase4_Logic_Of_Marking_Teammates_With_Wyrmfang==Phase4_Logics_Of_Marking_Teammates_With_Wyrmfang.According_To_Debuffs_1342_From_East_To_West_æ ¹æ®Debuffä»Žä¸œåˆ°è¥¿1342)
            {

                for (int i = 0; i < 8; ++i)
                {

                    if (P4ClawBuff[i] == 3)
                    {

                        int markIndex = -1;

                        if (P4OtherBuff[i] == 4)
                        {

                            markIndex = phase4_getMarkIndex(Phase4_Residue_Belongs_To_Dark_Eruption);

                        }

                        if (P4OtherBuff[i] == 5)
                        {

                            markIndex = phase4_getMarkIndex(Phase4_Residue_Belongs_To_Unholy_Darkness);

                        }

                        if (P4OtherBuff[i] == 1)
                        {

                            markIndex = phase4_getMarkIndex(Phase4_Residue_Belongs_To_Dark_Blizzard_III);

                        }

                        if (P4OtherBuff[i] == 3)
                        {

                            markIndex = phase4_getMarkIndex(Phase4_Residue_Belongs_To_Dark_Water_III);

                        }

                        if (markIndex != -1)
                        {

                            accessory.Method.Mark(accessory.Data.PartyList[i], phase4_markForPlayersWithWyrmfang_asAConstant[markIndex]);

                            debugOutput += $"i={i},markIndex={markIndex},phase4_markForPlayersWithWyrmfang_asAConstant[markIndex]={phase4_markForPlayersWithWyrmfang_asAConstant[markIndex]}\n";

                        }

                    }

                }

            }

            if (Phase4_Logic_Of_Marking_Teammates_With_Wyrmfang == Phase4_Logics_Of_Marking_Teammates_With_Wyrmfang.According_To_The_Priority_THD_æ ¹æ®THDä¼˜å…ˆçº§)
            {

                for (int i = 0, j = 0; i < 8; ++i)
                {

                    if (P4ClawBuff[i] == 3 && j < 4)
                    {

                        accessory.Method.Mark(accessory.Data.PartyList[i], phase4_markForPlayersWithWyrmfang_asAConstant[j]);

                        debugOutput += $"i={i},phase4_markForPlayersWithWyrmfang_asAConstant[j]={phase4_markForPlayersWithWyrmfang_asAConstant[j]}\n";

                        ++j;

                    }

                }

            }

            if (Phase4_Logic_Of_Marking_Teammates_With_Wyrmfang == Phase4_Logics_Of_Marking_Teammates_With_Wyrmfang.According_To_The_Priority_HTD_æ ¹æ®HTDä¼˜å…ˆçº§)
            {

                temporaryOrder = [2, 3, 0, 1, 4, 5, 6, 7];

                for (int i = 0, j = 0; i < temporaryOrder.Count; ++i)
                {

                    if (P4ClawBuff[temporaryOrder[i]] == 3 && j < 4)
                    {

                        accessory.Method.Mark(accessory.Data.PartyList[temporaryOrder[i]], phase4_markForPlayersWithWyrmfang_asAConstant[j]);

                        debugOutput += $"temporaryOrder[i]={temporaryOrder[i]},phase4_markForPlayersWithWyrmfang_asAConstant[j]={phase4_markForPlayersWithWyrmfang_asAConstant[j]}\n";

                        ++j;

                    }

                }

            }

            if (Phase4_Logic_Of_Marking_Teammates_With_Wyrmfang == Phase4_Logics_Of_Marking_Teammates_With_Wyrmfang.According_To_The_Priority_H1TDH2_æ ¹æ®H1TDH2ä¼˜å…ˆçº§)
            {

                temporaryOrder = [2, 0, 1, 4, 5, 6, 7, 3];

                for (int i = 0, j = 0; i < temporaryOrder.Count; ++i)
                {

                    if (P4ClawBuff[temporaryOrder[i]] == 3 && j < 4)
                    {

                        accessory.Method.Mark(accessory.Data.PartyList[temporaryOrder[i]], phase4_markForPlayersWithWyrmfang_asAConstant[j]);

                        debugOutput += $"temporaryOrder[i]={temporaryOrder[i]},phase4_markForPlayersWithWyrmfang_asAConstant[j]={phase4_markForPlayersWithWyrmfang_asAConstant[j]}\n";

                        ++j;

                    }

                }

            }

            }
            
            if(Phase4_Player_Type_To_Be_Marked==Phase4_Player_Types_To_Be_Marked.Both_The_Debuffs_Wyrmclaw_And_Wyrmfang_åœ£é¾™çˆªåœ£é¾™ç‰™ä¸¤ç§éƒ½æ ‡è®°
               ||
               Phase4_Player_Type_To_Be_Marked==Phase4_Player_Types_To_Be_Marked.Only_Wyrmclaw_The_Red_Debuff_ä»…åœ£é¾™çˆªçº¢è‰²Debuff) {

                temporaryOrder=[0,1,2,3,4,5,6,7];

                if(Phase4_Priority_Of_The_Players_With_Wyrmclaw==Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_THD_Order_æŒ‰THDé¡ºåº_èŽ«çµå–µ) {
                    
                    temporaryOrder=[0,1,2,3,4,5,6,7];
                    
                }
                
                if(Phase4_Priority_Of_The_Players_With_Wyrmclaw==Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_HTD_Order_æŒ‰HTDé¡ºåº_MMW) {
                    
                    temporaryOrder=[2,3,0,1,4,5,6,7];
                    
                }
                
                if(Phase4_Priority_Of_The_Players_With_Wyrmclaw==Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_H1TDH2_Order_æŒ‰H1TDH2é¡ºåº) {
                    
                    temporaryOrder=[2,0,1,4,5,6,7,3];
                    
                }

                List<MarkType> marksForShortWyrmclaw=[MarkType.Stop1,MarkType.Bind1];
                List<MarkType> marksForLongWyrmclaw=[MarkType.Stop2,MarkType.Bind2];

                if(Phase4_Logic_Of_Marking_Teammates_With_Wyrmclaw==Phase4_Logics_Of_Marking_Teammates_With_Wyrmclaw.Ignore1_And_Bind1_Go_West_ç¦æ­¢1å’Œé”é“¾1åŽ»è¥¿è¾¹_èŽ«çµå–µ) {
                    
                    marksForShortWyrmclaw=[MarkType.Stop1,MarkType.Stop2];
                    marksForLongWyrmclaw=[MarkType.Bind1,MarkType.Bind2];
                    
                }
                
                if(Phase4_Logic_Of_Marking_Teammates_With_Wyrmclaw==Phase4_Logics_Of_Marking_Teammates_With_Wyrmclaw.Ignore1_And_Ignore2_Go_West_ç¦æ­¢1å’Œç¦æ­¢2åŽ»è¥¿è¾¹) {
                    
                    marksForShortWyrmclaw=[MarkType.Stop1,MarkType.Bind1];
                    marksForLongWyrmclaw=[MarkType.Stop2,MarkType.Bind2];
                    
                }
                
                for(int i=0,j=0,k=0;i<temporaryOrder.Count;++i) {

                    if(P4ClawBuff[temporaryOrder[i]]==1&&j<2) {

                        accessory.Method.Mark(accessory.Data.PartyList[temporaryOrder[i]],marksForShortWyrmclaw[j]);

                        debugOutput+=$"temporaryOrder[i]={temporaryOrder[i]},marksForShortWyrmclaw[j]={marksForShortWyrmclaw[j]}\n";

                        ++j;

                    }
                    
                    if(P4ClawBuff[temporaryOrder[i]]==2&&k<2) {

                        accessory.Method.Mark(accessory.Data.PartyList[temporaryOrder[i]],marksForLongWyrmclaw[k]);

                        debugOutput+=$"temporaryOrder[i]={temporaryOrder[i]},marksForLongWyrmclaw[k]={marksForLongWyrmclaw[k]}\n";

                        ++k;

                    }

                }
            
            }
            
            if (Enable_Developer_Mode)
            {

                accessory.Method.SendChat($"""
                                           /e 
                                           {debugOutput}
                                           
                                           """);
                
                accessory.Log.Debug($"{debugOutput}");

            }

        }

        private int phase4_getMarkIndex(Phase4_Relative_Positions_Of_Residues currentPosition) {

            if(Phase4_Logic_Of_Marking_Teammates_With_Wyrmfang==Phase4_Logics_Of_Marking_Teammates_With_Wyrmfang.According_To_Debuffs_1234_From_East_To_West_æ ¹æ®Debuffä»Žä¸œåˆ°è¥¿1234) {
                
                if(currentPosition==Phase4_Relative_Positions_Of_Residues.Eastmost_æœ€ä¸œä¾§) {

                    return 0;

                }

                if(currentPosition==Phase4_Relative_Positions_Of_Residues.About_East_æ¬¡ä¸œä¾§) {

                    return 1;

                }

                if(currentPosition==Phase4_Relative_Positions_Of_Residues.About_West_æ¬¡è¥¿ä¾§) {

                    return 2;

                }

                if(currentPosition==Phase4_Relative_Positions_Of_Residues.Westmost_æœ€è¥¿ä¾§) {

                    return 3;

                }
                
            }
            
            if(Phase4_Logic_Of_Marking_Teammates_With_Wyrmfang==Phase4_Logics_Of_Marking_Teammates_With_Wyrmfang.According_To_Debuffs_1342_From_East_To_West_æ ¹æ®Debuffä»Žä¸œåˆ°è¥¿1342) {
                
                if(currentPosition==Phase4_Relative_Positions_Of_Residues.Eastmost_æœ€ä¸œä¾§) {

                    return 0;

                }

                if(currentPosition==Phase4_Relative_Positions_Of_Residues.About_East_æ¬¡ä¸œä¾§) {

                    return 2;

                }

                if(currentPosition==Phase4_Relative_Positions_Of_Residues.About_West_æ¬¡è¥¿ä¾§) {

                    return 3;

                }

                if(currentPosition==Phase4_Relative_Positions_Of_Residues.Westmost_æœ€è¥¿ä¾§) {

                    return 1;

                }
                
            }

            return -1;

        }

        [ScriptMethod(name: "P4_CrystallizeTime_BlueTetherCollect", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0085"], userControl: false)]
        public void P4_CrystallizeTime_BlueTetherCollect(Event @event, ScriptAccessory accessory)
        {
            if (parse!=43) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            P4BlueTether = PositionTo6Dir(pos, new(100, 0, 100)) % 3;
        }
        [ScriptMethod(name: "P4_CrystallizeTime_LampAOE", eventType: EventTypeEnum.Tether, eventCondition: ["Id:0085"])]
        public void P4_CrystallizeTime_LampAOE(Event @event, ScriptAccessory accessory)
        {
            if (parse!=43) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            Vector3 normalPos = new(pos.X, 0, 200 - pos.Z);
            Vector3 fastPos = new(100, 0, pos.Z > 100 ? 111 : 89);
            uint actualDuration = (0 <= Phase4_Drawing_Duration_Of_Normal_And_Delayed_Lights && Phase4_Drawing_Duration_Of_Normal_And_Delayed_Lights <= 13) ?
                                (uint)(1000 * Phase4_Drawing_Duration_Of_Normal_And_Delayed_Lights) :
                                3000;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_CrystallizeTime_LampAOE_Fast";
            dp.Scale = new(12);
            dp.Position = fastPos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_CrystallizeTime_LampAOE_Medium";
            dp.Scale = new(12);
            dp.Position = normalPos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 13000 - actualDuration;
            dp.DestoryAt = actualDuration;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_CrystallizeTime_LampAOE_Slow";
            dp.Scale = new(12);
            dp.Position = pos;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 18000 - actualDuration;
            dp.DestoryAt = actualDuration;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P4_CrystallizeTime_EarthStackRange", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2454"])]
        public void P4_CrystallizeTime_EarthStackRange(Event @event, ScriptAccessory accessory)
        {
            if (parse!=43) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_CrystallizeTime_EarthStackRange";
            dp.Scale = new(6);
            dp.Owner = tid;
            dp.Color = accessory.Data.DefaultSafeColor;
            dp.Delay = 14000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_CrystallizeTime_EarthStackRange_Crystal";
            dp.Scale = new(9.5f);
            dp.Owner = P4FragmentId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 14000;
            dp.DestoryAt = 3000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }
        [ScriptMethod(name: "P4_CrystallizeTime_SpiritTaker", eventType: EventTypeEnum.StatusAdd, eventCondition: ["StatusID:2452"])]
        public void P4_CrystallizeTime_SpiritTaker(Event @event, ScriptAccessory accessory)
        {
            if (parse!=43) return;
            if (!ParseObjectId(@event["TargetId"], out var tid)) return;
            if (tid != accessory.Data.Me) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P4_CrystallizeTime_SpiritTaker_Crystal";
            dp.Scale = new(8.5f);
            dp.Owner = P4FragmentId;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 3500;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            for (int i = 0; i < 8; i++)
            {
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_CrystallizeTime_SpiritTaker";
                dp.Scale = new(5);
                dp.Owner = accessory.Data.PartyList[i];
                dp.Color = accessory.Data.DefaultDangerColor;
                dp.DestoryAt = 3500;
                accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
            }


        }
        [ScriptMethod(name: "P4_CrystallizeTime_BuffPosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40293"])]
        public void P4_CrystallizeTime_BuffPosition(Event @event, ScriptAccessory accessory)
        {

            //3.5s after buff
            if (parse!=43) return;
            var myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            //Short Red
            if (P4ClawBuff[myIndex] == 1)
            {
                bool isHigh = true;

                if (Phase4_Priority_Of_The_Players_With_Wyrmclaw == Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_THD_Order_æŒ‰THDé¡ºåº_èŽ«çµå–µ)
                {

                    isHigh = (P4ClawBuff.IndexOf(1) == myIndex);

                }

                if (Phase4_Priority_Of_The_Players_With_Wyrmclaw == Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_HTD_Order_æŒ‰HTDé¡ºåº_MMW)
                {

                    List<int> temporaryPriority = [2, 3, 0, 1, 4, 5, 6, 7];

                    for (int i = 0; i < temporaryPriority.Count; ++i)
                    {

                        if (P4ClawBuff[temporaryPriority[i]] == 1)
                        {

                            if (temporaryPriority[i] == myIndex)
                            {

                                isHigh = true;

                            }

                            else
                            {

                                isHigh = false;

                            }

                            break;

                        }

                    }

                }

                if (Phase4_Priority_Of_The_Players_With_Wyrmclaw == Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_H1TDH2_Order_æŒ‰H1TDH2é¡ºåº)
                {

                    List<int> temporaryPriority = [2, 0, 1, 4, 5, 6, 7, 3];

                    for (int i = 0; i < temporaryPriority.Count; ++i)
                    {

                        if (P4ClawBuff[temporaryPriority[i]] == 1)
                        {

                            if (temporaryPriority[i] == myIndex)
                            {

                                isHigh = true;

                            }

                            else
                            {

                                isHigh = false;

                            }

                            break;

                        }

                    }

                }

                Vector3 dealpos = isHigh ? new(87, 0, 100) : new(113, 0, 100);

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_CrystallizeTime_BuffPosition_HitDragon";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 10500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                //High stack 088 085 -> 093 082
                //High idle 081 103 -> 081 097
                //Low stack 112 085 -> 107 082
                //Low idle 119 103 -> 119 97
                Vector3 dealpos2 = isHigh ? (P4BlueTether == 1 ? new(081, 0, 103) : new(088, 0, 085)) : (P4BlueTether == 1 ? new(112, 0, 085) : new(119, 0, 103));
                Vector3 dealpos3 = isHigh ? (P4BlueTether == 1 ? new(081, 0, 097) : new(093, 0, 082)) : (P4BlueTether == 1 ? new(107, 0, 082) : new(119, 0, 097));

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_CrystallizeTime_BuffPosition_pos2Preview";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = dealpos;
                dp.TargetPosition = dealpos2;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 10500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_CrystallizeTime_BuffPosition_pos2Position";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos2;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 10500;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_CrystallizeTime_BuffPosition_pos3Preview";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = dealpos2;
                dp.TargetPosition = dealpos3;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 13500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_CrystallizeTime_BuffPosition_pos3Position";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos3;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 13500;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
            }
            //Long Red
            if (P4ClawBuff[myIndex] == 2)
            {
                bool isHigh = true;

                if (Phase4_Priority_Of_The_Players_With_Wyrmclaw == Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_THD_Order_æŒ‰THDé¡ºåº_èŽ«çµå–µ)
                {

                    isHigh = (P4ClawBuff.IndexOf(2) == myIndex);

                }

                if (Phase4_Priority_Of_The_Players_With_Wyrmclaw == Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_HTD_Order_æŒ‰HTDé¡ºåº_MMW)
                {

                    List<int> temporaryPriority = [2, 3, 0, 1, 4, 5, 6, 7];

                    for (int i = 0; i < temporaryPriority.Count; ++i)
                    {

                        if (P4ClawBuff[temporaryPriority[i]] == 2)
                        {

                            if (temporaryPriority[i] == myIndex)
                            {

                                isHigh = true;

                            }

                            else
                            {

                                isHigh = false;

                            }

                            break;

                        }

                    }

                }

                if (Phase4_Priority_Of_The_Players_With_Wyrmclaw == Phase4_Priorities_Of_The_Players_With_Wyrmclaw.In_H1TDH2_Order_æŒ‰H1TDH2é¡ºåº)
                {

                    List<int> temporaryPriority = [2, 0, 1, 4, 5, 6, 7, 3];

                    for (int i = 0; i < temporaryPriority.Count; ++i)
                    {

                        if (P4ClawBuff[temporaryPriority[i]] == 2)
                        {

                            if (temporaryPriority[i] == myIndex)
                            {

                                isHigh = true;

                            }

                            else
                            {

                                isHigh = false;

                            }

                            break;

                        }

                    }

                }

                Vector3 dealpos1 = isHigh ? new(088.5f, 0, 115.5f) : new(111.5f, 0, 115.5f);
                Vector3 dealpos2 = isHigh ? new(090.2f, 0, 117.0f) : new(109.8f, 0, 117.0f);
                Vector3 dealpos3 = isHigh ? new(092.5f, 0, 118.0f) : new(107.5f, 0, 118.0f);
                Vector3 dealpos4 = isHigh ? new(092.53f, 0, 110.40f) : new(107.47f, 0, 110.40f);
                // The previous coordinates were: isHigh ? new(092.0f, 0, 110.0f) : new(108.0f, 0, 110.0f);

                // ----- 0s -> 7.5s -----

                var dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_CrystallizeTime_BuffPosition_DodgeAC";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos1;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_CrystallizeTime_BuffPosition_DodgeAC->Knockback";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = dealpos1;
                dp.TargetPosition = dealpos2;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 7500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                // ----- -----

                // ----- 7.5s -> 10.5s -----

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_CrystallizeTime_BuffPosition_Knockback";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos2;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 7500;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_CrystallizeTime_BuffPosition_Knockback->DodgeDiagonal";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = dealpos2;
                dp.TargetPosition = dealpos3;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 10500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                // ----- -----

                // ----- 10.5s -> 13s -----

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_CrystallizeTime_BuffPosition_DodgeDiagonal";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos3;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 10500;
                dp.DestoryAt = 2500;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_CrystallizeTime_BuffPosition_DodgeDiagonal->HitHead";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Position = dealpos3;
                dp.TargetPosition = dealpos4;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.DestoryAt = 13000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                // ----- -----

                // ----- 13s -> 16s -----

                dp = accessory.Data.GetDefaultDrawProperties();
                dp.Name = "P4_CrystallizeTime_BuffPosition_HitHead";
                dp.Scale = new(2);
                dp.ScaleMode |= ScaleMode.YByDistance;
                dp.Owner = accessory.Data.Me;
                dp.TargetPosition = dealpos4;
                dp.Color = accessory.Data.DefaultSafeColor;
                dp.Delay = 13000;
                dp.DestoryAt = 3000;
                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                // ----- -----

                // There were some issues in the guidance here which is for the players with long Wyrmclaw debuff.
                // Cicero has adjusted the process a little bit, and the issues has been fixed now.

            }
            //Blue
            if (P4ClawBuff[myIndex] == 3)
            {
                if (P4OtherBuff[myIndex] == 4)
                {
                    Vector3 dealpos1 = P4BlueTether == 1 ? new(112, 0, 85) : new(88, 0, 85);
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_CrystallizeTime_BuffPosition_DodgeLamp1";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = dealpos1;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 14500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                else
                {
                    Vector3 dealpos1 = P4BlueTether == 1 ? new(88, 0, 115) : new(112, 0, 115);
                    Vector3 dealpos2 = P4BlueTether == 1 ? new(090.8f, 0, 116.0f) : new(109.2f, 0, 116.0f);
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_CrystallizeTime_BuffPosition_DodgeLampAC";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = dealpos1;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 7500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);

                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_CrystallizeTime_BuffPosition_DodgeAC->Knockback";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Position = dealpos1;
                    dp.TargetPosition = dealpos2;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 7500;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                    dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_CrystallizeTime_BuffPosition_Knockback";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = dealpos2;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.Delay = 7500;
                    dp.DestoryAt = 3000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
            }
        }
        [ScriptMethod(name: "P4_CrystallizeTime_ReturnPosition", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40251"])]
        public void P4_CrystallizeTime_ReturnPosition(Event @event, ScriptAccessory accessory)
        {
            if (parse!=43) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            P4WaterPos.Add(pos);
            if (P4WaterPos.Count == 1) return;
            var myindex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            Vector3 centre = new(100, 0, 100);
            if (Phase4_Position_Before_Knockback == Phase4_Positions_Before_Knockback.Normal_æ­£æ”»_èŽ«çµå–µä¸ŽMMW)
            {
                var dir8 = PositionTo8Dir((P4WaterPos[0] + P4WaterPos[1]) / 2, centre) - 1;
                Vector3 mtPos = new(107, 0, 88);
                Vector3 stPos = new(112, 0, 93);
                Vector3 mtgPos = new(106, 0, 92);
                Vector3 stgPos = new(108, 0, 94);
                if (myindex == 0)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_CrystallizeTime_ReturnPosition_MT";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(mtPos, centre, float.Pi / 4 * dir8);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                if (myindex == 1)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_CrystallizeTime_ReturnPosition_ST";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(stPos, centre, float.Pi / 4 * dir8);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                if (myindex == 2 || myindex == 4 || myindex == 6)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_CrystallizeTime_ReturnPosition_MTG";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(mtgPos, centre, float.Pi / 4 * dir8);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                if (myindex == 3 || myindex == 5 || myindex == 7)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_CrystallizeTime_ReturnPosition_STG";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = RotatePoint(stgPos, centre, float.Pi / 4 * dir8);
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
            }
            if (Phase4_Position_Before_Knockback == Phase4_Positions_Before_Knockback.Y_Formation_Japanese_PF_æ—¥æœé‡Žé˜ŸYå­—é˜Ÿå½¢)
            {
                Vector3 mtPos = P4WaterPos[1].Z < 100 ? new(92, 0, 90) : new(108, 0, 110);
                Vector3 stPos = P4WaterPos[1].Z < 100 ? new(108, 0, 90) : new(92, 0, 110);
                Vector3 gPos = P4WaterPos[1].Z < 100 ? new(100, 0, 96) : new(100, 0, 104);
                if (myindex == 0)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_CrystallizeTime_ReturnPosition_MT";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = mtPos;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                if (myindex == 1)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_CrystallizeTime_ReturnPosition_ST";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = stPos;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
                if (myindex == 2 || myindex == 3 || myindex == 4 || myindex == 5 || myindex == 6 || myindex == 7)
                {
                    var dp = accessory.Data.GetDefaultDrawProperties();
                    dp.Name = "P4_CrystallizeTime_ReturnPosition_MTG";
                    dp.Scale = new(2);
                    dp.ScaleMode |= ScaleMode.YByDistance;
                    dp.Owner = accessory.Data.Me;
                    dp.TargetPosition = gPos;
                    dp.Color = accessory.Data.DefaultSafeColor;
                    dp.DestoryAt = 9000;
                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, dp);
                }
            }

        }

        [ScriptMethod(name: "Phase4 Acquire IDs Of Drachen Wanderers",
            eventType: EventTypeEnum.AddCombatant,
            eventCondition: ["DataId:17836"],
            userControl: false)]

        public void Phase4_Acquire_IDs_Of_Drachen_Wanderers_èŽ·å–åœ£é¾™æ°”æ¯ID(Event @event, ScriptAccessory accessory)
        {

            if (parse!=43)
            {

                return;

            }

            lock (phase4_readwriteLockOfDrachenWandererIds_AsAConstant)
            {

                if (!ParseObjectId(@event["SourceId"], out var sourceId)) return;
                var spos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                if (spos.X < 100)
                {
                    _cry.LeftWyrmSid = sourceId;
                    accessory.Log.Debug($"CrystallizeTime: Recorded left dragon head {spos} ID {sourceId}");
                }
                else
                {
                    _cry.RightWyrmSid = sourceId;
                    accessory.Log.Debug($"CrystallizeTime: Recorded right dragon head {spos} ID {sourceId}");
                }


                if ((_cry.LeftWyrmSid != 0) && (_cry.RightWyrmSid != 0))
                {
                    _events[3].Set();
                    accessory.Log.Debug($"CrystallizeTime: Left and right dragon heads recorded.");
                }

                if (phase4_id1OfTheDrachenWanderers.Equals(""))
                {

                    phase4_id1OfTheDrachenWanderers = @event["SourceId"];

                }

                else
                {

                    if (phase4_id2OfTheDrachenWanderers.Equals(""))
                    {

                        phase4_id2OfTheDrachenWanderers = @event["SourceId"];

                    }

                }

            }

        }

        [ScriptMethod(name: "Phase4 Hitbox Of Drachen Wanderers",
            eventType: EventTypeEnum.AddCombatant,
            eventCondition: ["DataId:17836"])]

        public void Phase4_Hitbox_Of_Drachen_Wanderers_åœ£é¾™æ°”æ¯ç¢°æ’žç®±(Event @event, ScriptAccessory accessory)
        {

            if (parse!=43)
            {

                return;

            }

            if (!ParseObjectId(@event["SourceId"], out var sourceId))
            {

                return;

            }

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = $"Phase4_Hitbox_Of_Drachen_Wanderers_{sourceId}";
            currentProperty.Scale = new(2f, Phase4_Length_Of_Drachen_Wanderer_Hitboxes >= 0 ?
                                                Phase4_Length_Of_Drachen_Wanderer_Hitboxes :
                                                1.5f);
            currentProperty.Color = Phase4_Colour_Of_Drachen_Wanderer_Hitboxes.V4.WithW(25f);
            currentProperty.Offset = new(0f, 0f, -1f);
            currentProperty.Owner = sourceId;
            currentProperty.DestoryAt = 34000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Displacement, currentProperty);

        }

        [ScriptMethod(name: "Phase4 Explosion Range Of Drachen Wanderers",
            eventType: EventTypeEnum.AddCombatant,
            eventCondition: ["DataId:17836"])]

        public void Phase4_Explosion_Range_Of_Drachen_Wanderers_åœ£é¾™æ°”æ¯çˆ†ç‚¸èŒƒå›´(Event @event, ScriptAccessory accessory)
        {

            if (parse!=43)
            {

                return;

            }

            if (!ParseObjectId(@event["SourceId"], out var sourceId))
            {

                return;

            }

            _events[3].WaitOne();

            // Usami:
            // Constantly showing the explosion range can cause players hitting the dragon head to misjudge the lamp's circle AoE.
            // Idea:
            // When the player is a red ice or red wind, do not display the explosion range of the dragon head on their side, instead use a very small green circle to indicate the dragon head position. Because the explosion of the dragon head on their side will affect lamp judgment.
            // After the red ice on one side hits the head, they join the group; the other side's red ice needs to observe the lamp's explosion on their side and then quickly cross, unrelated to the dragon head.
            // Red wind needs to observe the lamp's explosion and then quickly cross to hit the head; two similar drawings can confuse judgment.
            // For players with blue buff, no modification. Blue dark needs to dodge the dragon head explosion range, the other three are not involved in the dragon head path at all, keeping them is fine.

            var myIndex = accessory.GetMyIndex();
            bool isSameSideWyrm = false;
            if (sourceId == _cry.LeftWyrmSid)
                isSameSideWyrm = (myIndex == _cry.LeftIcePlayerIdx) || (myIndex == _cry.LeftWindPlayerIdx);
            else if (sourceId == _cry.RightWyrmSid)
                isSameSideWyrm = (myIndex == _cry.RightIcePlayerIdx) || (myIndex == _cry.RightWindPlayerIdx);

            var currentProperty = accessory.Data.GetDefaultDrawProperties();
            currentProperty.Name = $"Phase4_Explosion_Range_Of_Drachen_Wanderers_{sourceId}";
            currentProperty.Scale = isSameSideWyrm ? new(1.5f) : new(12);
            currentProperty.Owner = sourceId;
            currentProperty.Color = isSameSideWyrm ? accessory.Data.DefaultSafeColor.WithW(3f) : accessory.Data.DefaultDangerColor;
            currentProperty.DestoryAt = 34000;
            accessory.Method.SendDraw(isSameSideWyrm ? DrawModeEnum.Imgui : DrawModeEnum.Default, DrawTypeEnum.Circle, currentProperty);


        }

        [ScriptMethod(name: "Phase4 Remove Hitboxes And Explosion Ranges Of Drachen Wanderers",
            eventType: EventTypeEnum.RemoveCombatant,
            eventCondition: ["DataId:17836"],
            userControl: false)]

        public void Phase4_Remove_Hitboxes_And_Explosion_Ranges_Of_Drachen_Wanderers_ç§»é™¤åœ£é¾™æ°”æ¯ç¢°æ’žç®±ä¸Žçˆ†ç‚¸èŒƒå›´(Event @event, ScriptAccessory accessory)
        {

            if (parse!=43)
            {

                return;

            }

            if (!ParseObjectId(@event["SourceId"], out var sourceId))
            {

                return;

            }

            accessory.Method.RemoveDraw($"Phase4_Hitbox_Of_Drachen_Wanderers_{sourceId}");
            accessory.Method.RemoveDraw($"Phase4_Explosion_Range_Of_Drachen_Wanderers_{sourceId}");

        }

        [ScriptMethod(name: "Phase4 Remove Hitboxes And Explosion Ranges Of Drachen Wanderers In Advance",
            eventType: EventTypeEnum.StatusRemove,
            eventCondition: ["StatusID:3263"],
            userControl: false)]

        // The ObjectChanged event with the field "Operate" as "Remove" would be triggered almost three seconds after the Drachen Wanderer is gone.
        // If the drawing removal relies on the event, it would be too late and may cause confusion.
        // Here is an optimized method for players with the Wyrmclaw debuff (the red debuff), which is to monitor the StatusRemove events of the Wyrmclaw debuff and acquire the closest Drachen Wanderer.
        // Obviously, the method would not help if a player with the Wyrmfang debuff (the blue debuff) hits a Drachen Wanderer. However, that's already a wipe, so whatever.
        // Thanks to Cyf5119 for providing a Dalamud way to detect if the player is dead, so that the method would skip the StatusRemove events caused by death.

        public void Phase4_Remove_Hitboxes_And_Explosion_Ranges_Of_Drachen_Wanderers_In_Advance_æå‰ç§»é™¤åœ£é¾™æ°”æ¯ç¢°æ’žç®±ä¸Žçˆ†ç‚¸èŒƒå›´(Event @event, ScriptAccessory accessory)
        {

            if (parse!=43)
            {

                return;

            }

            if (!ParseObjectId(@event["TargetId"], out var targetId))
            {

                return;

            }

            var targetObject = accessory.Data.Objects.SearchById(targetId);

            if (targetObject == null)
            {

                return;

            }

            if(targetObject.IsDead) {
                // Ignore the situations that the debuff was removed due to a death.

                return;

            }

            System.Threading.Thread.MemoryBarrier();

            ++phase4_timesTheWyrmclawDebuffWasRemoved;

            System.Threading.Thread.MemoryBarrier();

            if (phase4_timesTheWyrmclawDebuffWasRemoved < 3 || phase4_timesTheWyrmclawDebuffWasRemoved > 4)
            {

                return;

            }

            if (!ParseObjectId(phase4_id1OfTheDrachenWanderers, out var drachenWandererId1))
            {

                return;

            }

            if (!ParseObjectId(phase4_id2OfTheDrachenWanderers, out var drachenWandererId2))
            {

                return;

            }

            var drachenWandererObject1 = accessory.Data.Objects.SearchById(drachenWandererId1);

            if (drachenWandererObject1 == null)
            {

                return;

            }

            var drachenWandererObject2 = accessory.Data.Objects.SearchById(drachenWandererId2);

            if (drachenWandererObject2 == null)
            {

                return;

            }

            if (Vector3.Distance(targetObject.Position, drachenWandererObject1.Position)
               <=
               Vector3.Distance(targetObject.Position, drachenWandererObject2.Position))
            {

                accessory.Method.RemoveDraw($"Phase4_Hitbox_Of_Drachen_Wanderers_{drachenWandererId1}");
                accessory.Method.RemoveDraw($"Phase4_Explosion_Range_Of_Drachen_Wanderers_{drachenWandererId1}");

            }

            else
            {

                accessory.Method.RemoveDraw($"Phase4_Hitbox_Of_Drachen_Wanderers_{drachenWandererId2}");
                accessory.Method.RemoveDraw($"Phase4_Explosion_Range_Of_Drachen_Wanderers_{drachenWandererId2}");

            }

        }

        [ScriptMethod(name: "Phase4 Tidal Light",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:regex:^(40252|40253)$"])]

        public void Phase4_Tidal_Light_å…‰ä¹‹æ½®æ±(Event @event, ScriptAccessory accessory)
        {

            if (parse!=43)
            {

                return;

            }

            if (!ParseObjectId(@event["SourceId"], out var sourceId))
            {

                return;

            }

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Owner = sourceId;
            currentProperty.Offset = new Vector3(0, 0, -10);
            currentProperty.Scale = new(40, 10);
            currentProperty.DestoryAt = 2100;
            currentProperty.Color = Phase4_Colour_Of_Tidal_Light.V4.WithW(3f);

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

        }

        [ScriptMethod(name: "Phase4 Determine Relative Positions Of Residues",
            eventType: EventTypeEnum.ObjectChanged,
            eventCondition: ["DataId:2014529"],
            userControl: false)]

        public void Phase4_Determine_Relative_Positions_Of_Residues_ç¡®å®šç™½åœˆç›¸å¯¹ä½ç½®(Event @event, ScriptAccessory accessory)
        {

            if (parse!=43)
            {

                return;

            }

            if (!@event["Operate"].Equals("Add"))
            {

                return;

            }

            if (!ParseObjectId(@event["SourceId"], out var sourceId))
            {

                return;

            }

            var sourcePositionInJson = JObject.Parse(@event["SourcePosition"]);
            float currentX = sourcePositionInJson["X"]?.Value<float>() ?? 0;

            if (currentX < 100)
            {

                if (phase4_residueIdsFromEastToWest[3] != 0)
                {

                    lock (phase4_residueIdsFromEastToWest)
                    {

                        phase4_residueIdsFromEastToWest[2] = sourceId;
                        // The about right one while facing south.

                    }

                }

                else
                {

                    lock (phase4_residueIdsFromEastToWest)
                    {

                        phase4_residueIdsFromEastToWest[3] = sourceId;
                        // The rightmost one while facing south.

                    }

                }

            }

            if (currentX > 100)
            {

                if (phase4_residueIdsFromEastToWest[0] != 0)
                {

                    lock (phase4_residueIdsFromEastToWest)
                    {

                        phase4_residueIdsFromEastToWest[1] = sourceId;
                        // The about left one while facing south.

                    }

                }

                else
                {

                    lock (phase4_residueIdsFromEastToWest)
                    {

                        phase4_residueIdsFromEastToWest[0] = sourceId;
                        // The leftmost one while facing south.

                    }

                }

            }

            if (Enable_Developer_Mode)
            {

                accessory.Method.SendChat($"""
                                           /e 
                                           @event["SourceId"]={@event["SourceId"]}
                                           sourceId={sourceId}
                                           @event["SourcePosition"]={@event["SourcePosition"]}
                                           currentX={currentX}
                                           
                                           """);

            }

        }

        [ScriptMethod(name: "Phase4 Guidance Of Residues",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:regex:^(40252|40253)$"])]

        public void Phase4_Guidance_Of_Residues_ç™½åœˆæŒ‡è·¯(Event @event, ScriptAccessory accessory)
        {

            if (parse!=43)
            {

                return;

            }

            if (phase4_guidanceOfResiduesHasBeenGenerated)
            {

                return;

            }

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            Phase4_Relative_Positions_Of_Residues relativePositionOfMyResidue = phase4_getRelativePosition(myIndex);
            ulong idOfMyResidue = phase4_getResidueId(relativePositionOfMyResidue);

            if (Enable_Developer_Mode)
            {

                if (Phase4_Logic_Of_Residue_Guidance == Phase4_Logics_Of_Residue_Guidance.According_To_Signs_On_Me_æ ¹æ®æˆ‘èº«ä¸Šçš„ç›®æ ‡æ ‡è®°_èŽ«çµå–µå’ŒMMW)
                {

                    accessory.Method.SendChat($"""
                                               /e 
                                               phase4_residueIdsFromEastToWest[]={phase4_residueIdsFromEastToWest[0]},{phase4_residueIdsFromEastToWest[1]},{phase4_residueIdsFromEastToWest[2]},{phase4_residueIdsFromEastToWest[3]}
                                               phase4_marksOfPlayersWithWyrmfang[myIndex]={phase4_marksOfPlayersWithWyrmfang[myIndex]}
                                               relativePositionOfMyResidue={relativePositionOfMyResidue}
                                               idOfMyResidue={idOfMyResidue}

                                               """);

                }

                else
                {

                    accessory.Method.SendChat($"""
                                               /e 
                                               phase4_residueIdsFromEastToWest[]={phase4_residueIdsFromEastToWest[0]},{phase4_residueIdsFromEastToWest[1]},{phase4_residueIdsFromEastToWest[2]},{phase4_residueIdsFromEastToWest[3]}
                                               P4ClawBuff={P4ClawBuff[myIndex]}
                                               P4OtherBuff={P4OtherBuff[myIndex]}
                                               relativePositionOfMyResidue={relativePositionOfMyResidue}
                                               idOfMyResidue={idOfMyResidue}

                                               """);

                }

            }

            if (relativePositionOfMyResidue != Phase4_Relative_Positions_Of_Residues.Unknown_æœªçŸ¥
               &&
               idOfMyResidue != 0)
            {

                var currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase4_Guidance_Of_Residues";
                currentProperty.Scale = new(2);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.Color = Phase4_Colour_Of_Residue_Guidance.V4.WithW(1f);
                currentProperty.DestoryAt = 23000;

                var residueObject = accessory.Data.Objects.SearchById(idOfMyResidue);

                if (residueObject != null)
                {

                    phase4_guidanceOfResiduesHasBeenGenerated = true;

                    currentProperty.TargetPosition = residueObject.Position;

                    accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                    if (Enable_Text_Prompts)
                    {

                        accessory.Method.TextInfo(phase4_getResidueDescription(relativePositionOfMyResidue), 2500);

                    }

                    accessory.TTS($"{phase4_getResidueDescription(relativePositionOfMyResidue)}",
                                    Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                    if (Enable_Developer_Mode)
                    {

                        accessory.Method.SendChat($"""
                                                   /e 
                                                   residueObject.Position={residueObject.Position}
                                                   
                                                   """);

                    }

                }

            }

        }

        [ScriptMethod(name: "Phase4 Remove Guidance Of Residues",
            eventType: EventTypeEnum.StatusRemove,
            eventCondition: ["StatusID:3264"],
            userControl: false)]

        public void Phase4_Remove_Guidance_Of_Residues_ç§»é™¤ç™½åœˆæŒ‡è·¯(Event @event, ScriptAccessory accessory)
        {

            if (parse!=43)
            {

                return;

            }

            if (!ParseObjectId(@event["TargetId"], out var targetId))
            {

                return;

            }

            if (targetId != accessory.Data.Me)
            {

                return;

            }

            accessory.Method.RemoveDraw("Phase4_Guidance_Of_Residues");

        }

        [ScriptMethod(name: "Phase4 Highlight Of Residues",
            eventType: EventTypeEnum.ObjectChanged,
            eventCondition: ["DataId:2014529"])]

        public void Phase4_Highlight_Of_Residues_ç™½åœˆé«˜äº®(Event @event, ScriptAccessory accessory)
        {

            if (parse!=43)
            {

                return;

            }

            if (!@event["Operate"].Equals("Add"))
            {

                return;

            }

            if (!ParseObjectId(@event["SourceId"], out var sourceId))
            {

                return;

            }

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = $"Phase4_Highlight_Of_Residues_{sourceId}";
            currentProperty.Scale = new(1f);
            currentProperty.InnerScale = new(0.8f);
            currentProperty.Color = accessory.Data.DefaultDangerColor.WithW(25f);
            currentProperty.Radian = float.Pi * 2;
            currentProperty.Owner = sourceId;
            currentProperty.DestoryAt = 17000;

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Donut, currentProperty);

        }

        [ScriptMethod(name: "Phase4 Remove Highlights Of Residues",
            eventType: EventTypeEnum.ObjectChanged,
            eventCondition: ["DataId:2014529"],
            userControl: false)]

        public void Phase4_Remove_Highlights_Of_Residues_ç§»é™¤ç™½åœˆé«˜äº®(Event @event, ScriptAccessory accessory)
        {

            if (parse!=43)
            {

                return;

            }

            if (!@event["Operate"].Equals("Remove"))
            {

                return;

            }

            if (!ParseObjectId(@event["SourceId"], out var sourceId))
            {

                return;

            }

            accessory.Method.RemoveDraw($"Phase4_Highlight_Of_Residues_{sourceId}");

            ulong idOfMyResidue = phase4_getResidueId(phase4_getRelativePosition(accessory.Data.PartyList.IndexOf(accessory.Data.Me)));

            if (idOfMyResidue != 0
               &&
               idOfMyResidue == sourceId)
            {

                accessory.Method.RemoveDraw("Phase4_Guidance_Of_Residues");

            }

        }

        [ScriptMethod(name: "Phase4 Remove Highlights Of Residues In Advance",
            eventType: EventTypeEnum.StatusRemove,
            eventCondition: ["StatusID:3264"],
            userControl: false)]

        // The background and implementation are almost the same as the removal of Hitboxes and Explosion Ranges before.
        // Please refer to the comments following that method for details.

        public void Phase4_Remove_Highlights_Of_Residues_In_Advance_æå‰ç§»é™¤ç™½åœˆé«˜äº®(Event @event, ScriptAccessory accessory)
        {

            if (parse!=43)
            {

                return;

            }

            if (!ParseObjectId(@event["TargetId"], out var targetId))
            {

                return;

            }

            var targetObject = accessory.Data.Objects.SearchById(targetId);

            if (targetObject == null)
            {

                return;

            }

            Vector3 targetPosition = targetObject.Position;

            if(targetObject.IsDead) {

                return;

            }

            int closestResidue = -1;
            float distanceToTheClosestResidue = float.PositiveInfinity;

            for (int i = 0; i < 4; ++i)
            {

                var residueObject = accessory.Data.Objects.SearchById(phase4_residueIdsFromEastToWest[i]);

                if (residueObject != null)
                {

                    if (Vector3.Distance(targetPosition, residueObject.Position) < distanceToTheClosestResidue)
                    {

                        closestResidue = i;
                        distanceToTheClosestResidue = Vector3.Distance(targetPosition, residueObject.Position);

                    }

                }

            }

            if (0 <= closestResidue && closestResidue <= 3)
            {

                accessory.Method.RemoveDraw($"Phase4_Highlight_Of_Residues_{phase4_residueIdsFromEastToWest[closestResidue]}");

                if (targetId != accessory.Data.Me)
                {

                    ulong idOfMyResidue = phase4_getResidueId(phase4_getRelativePosition(accessory.Data.PartyList.IndexOf(accessory.Data.Me)));

                    if (idOfMyResidue != 0
                       &&
                       idOfMyResidue == phase4_residueIdsFromEastToWest[closestResidue])
                    {

                        accessory.Method.RemoveDraw("Phase4_Guidance_Of_Residues");

                    }

                }

            }

        }

        [ScriptMethod(name: "Phase4 Record Signs On Party Members",
            eventType: EventTypeEnum.Marker,
            userControl: false)]

        public void Phase4_Record_Signs_On_Party_Members_è®°å½•å°é˜Ÿé˜Ÿå‘˜çš„ç›®æ ‡æ ‡è®°(Event @event, ScriptAccessory accessory)
        {

            if (parse!=43)
            {

                return;

            }

            if (!ParseObjectId(@event["TargetId"], out var targetId))
            {

                return;

            }

            if (!int.TryParse(@event["Id"], out var sign))
            {

                return;

            }

            MarkType currentType = sign switch
            {
                1 => MarkType.Attack1,
                2 => MarkType.Attack2,
                3 => MarkType.Attack3,
                4 => MarkType.Attack4,
                9 => MarkType.Stop1,
                10 => MarkType.Stop2,
                6 => MarkType.Bind1,
                7 => MarkType.Bind2,
                _ => MarkType.Cross
            };

            int currentIndex = accessory.Data.PartyList.IndexOf(((uint)targetId));

            if (0 <= currentIndex && currentIndex <= 7)
            {

                lock (phase4_marksOfPlayersWithWyrmfang)
                {

                    phase4_marksOfPlayersWithWyrmfang[currentIndex] = currentType;

                }

            }

        }

        private Phase4_Relative_Positions_Of_Residues phase4_getRelativePosition(int currentIndex)
        {

            if (currentIndex < 0 || currentIndex > 7)
            {

                return Phase4_Relative_Positions_Of_Residues.Unknown_æœªçŸ¥;

            }

            if (P4ClawBuff[currentIndex] == 1 || P4ClawBuff[currentIndex] == 2)
            {
                // 1 stands for short Wyrmclaw (the red debuff), 2 stands for long Wyrmclaw (also the red debuff).

                return Phase4_Relative_Positions_Of_Residues.Unknown_æœªçŸ¥;

            }

            if (Phase4_Logic_Of_Residue_Guidance == Phase4_Logics_Of_Residue_Guidance.According_To_Debuffs_æ ¹æ®Debuff)
            {

                if (P4ClawBuff[currentIndex] == 3)
                {
                    // 3 stands for Wyrmfang (the blue debuff).

                    if (P4OtherBuff[currentIndex] == 4)
                    {
                        // 4 stands for Dark Eruption.

                        return Phase4_Residue_Belongs_To_Dark_Eruption;

                    }

                    if (P4OtherBuff[currentIndex] == 5)
                    {
                        // 5 stands for Unholy Darkness.

                        return Phase4_Residue_Belongs_To_Unholy_Darkness;

                    }

                    if (P4OtherBuff[currentIndex] == 1)
                    {
                        // 1 stands for Dark Blizzard III.

                        return Phase4_Residue_Belongs_To_Dark_Blizzard_III;

                    }

                    if (P4OtherBuff[currentIndex] == 3)
                    {
                        // 3 stands for Dark Water III.

                        return Phase4_Residue_Belongs_To_Dark_Water_III;

                    }

                }

            }

            if (Phase4_Logic_Of_Residue_Guidance == Phase4_Logics_Of_Residue_Guidance.According_To_Signs_On_Me_æ ¹æ®æˆ‘èº«ä¸Šçš„ç›®æ ‡æ ‡è®°_èŽ«çµå–µå’ŒMMW)
            {


                if (P4ClawBuff[currentIndex] == 3)
                {

                    if (phase4_marksOfPlayersWithWyrmfang[currentIndex] == MarkType.Attack1)
                    {

                        return Phase4_Residue_Belongs_To_Attack1;

                    }

                    if (phase4_marksOfPlayersWithWyrmfang[currentIndex] == MarkType.Attack2)
                    {

                        return Phase4_Residue_Belongs_To_Attack2;

                    }

                    if (phase4_marksOfPlayersWithWyrmfang[currentIndex] == MarkType.Attack3)
                    {

                        return Phase4_Residue_Belongs_To_Attack3;

                    }

                    if (phase4_marksOfPlayersWithWyrmfang[currentIndex] == MarkType.Attack4)
                    {

                        return Phase4_Residue_Belongs_To_Attack4;

                    }

                }

            }

            return Phase4_Relative_Positions_Of_Residues.Unknown_æœªçŸ¥;
            // Just a placeholder and should never be reached.

        }

        private ulong phase4_getResidueId(Phase4_Relative_Positions_Of_Residues relativePosition)
        {

            switch (relativePosition)
            {

                case (Phase4_Relative_Positions_Of_Residues.Eastmost_æœ€ä¸œä¾§):
                    {

                        return phase4_residueIdsFromEastToWest[0];

                    }

                case (Phase4_Relative_Positions_Of_Residues.About_East_æ¬¡ä¸œä¾§):
                    {

                        return phase4_residueIdsFromEastToWest[1];

                    }

                case (Phase4_Relative_Positions_Of_Residues.About_West_æ¬¡è¥¿ä¾§):
                    {

                        return phase4_residueIdsFromEastToWest[2];

                    }

                case (Phase4_Relative_Positions_Of_Residues.Westmost_æœ€è¥¿ä¾§):
                    {

                        return phase4_residueIdsFromEastToWest[3];

                    }

                case (Phase4_Relative_Positions_Of_Residues.Unknown_æœªçŸ¥):
                    {

                        return 0;

                    }

                default:
                    {

                        return 0;
                        // Just a placeholder and should never be reached.

                    }

            }

        }

        private String phase4_getResidueDescription(Phase4_Relative_Positions_Of_Residues relativePosition)
        {

            switch (relativePosition)
            {

                case (Phase4_Relative_Positions_Of_Residues.Eastmost_æœ€ä¸œä¾§):
                    {

                        if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                        {

                            return "æœ€å·¦/æœ€ä¸œ";

                        }

                        if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                        {

                            return "Leftmost/Eastmost";

                        }

                        return "";
                        // Just a placeholder and should never be reached.

                    }

                case (Phase4_Relative_Positions_Of_Residues.About_East_æ¬¡ä¸œä¾§):
                    {

                        if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                        {

                            return "æ¬¡å·¦/æ¬¡ä¸œ";

                        }

                        if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                        {

                            return "About left/About east";

                        }

                        return "";
                        // Just a placeholder and should never be reached.

                    }

                case (Phase4_Relative_Positions_Of_Residues.About_West_æ¬¡è¥¿ä¾§):
                    {

                        if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                        {

                            return "æ¬¡å³/æ¬¡è¥¿";

                        }

                        if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                        {

                            return "About right/About west";

                        }

                        return "";
                        // Just a placeholder and should never be reached.

                    }

                case (Phase4_Relative_Positions_Of_Residues.Westmost_æœ€è¥¿ä¾§):
                    {

                        if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                        {

                            return "æœ€å³/æœ€è¥¿";

                        }

                        if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                        {

                            return "Rightmost/Westmost";

                        }

                        return "";
                        // Just a placeholder and should never be reached.

                    }

                case (Phase4_Relative_Positions_Of_Residues.Unknown_æœªçŸ¥):
                    {

                        return "";

                    }

                default:
                    {

                        return "";
                        // Just a placeholder and should never be reached.

                    }

            }

        }

        [ScriptMethod(name: "Phase2 Reset Semaphores After Crystallize Time",
            eventType: EventTypeEnum.ActionEffect,
            eventCondition: ["ActionId:40332"],
            userControl: false,
            suppress: 10000)]

        public void Phase2_Reset_Semaphores_After_Crystallize_Time_æ—¶é—´ç»“æ™¶åŽé‡ç½®ä¿¡å·ç¯(Event @event, ScriptAccessory accessory)
        {

            if (parse!=43)
            {

                return;

            }

            phase4_semaphoreMajorDebuffsWereConfirmed = new System.Threading.AutoResetEvent(false);
            phase4_semaphoreIncidentalDebuffsWereConfirmed = new System.Threading.AutoResetEvent(false);

            if (Phase4_Mark_Players_During_The_Second_Half)
            {

                accessory.Method.MarkClear();

            }

        }

        #endregion

        #region Phase_5

        [ScriptMethod(name: "----- Phase 5 ----- (No actual meaning for this toggle)",
            eventType: EventTypeEnum.NpcYell,
            eventCondition: ["I lift my lamp beside the golden door!",
                            "æˆ‘åœ¨é‡‘é—¨æ—ä¸ºä»–ä»¬å°†ç¯ä¸¾èµ·!"])]

        public void Phase5_Placeholder(Event @event, ScriptAccessory accessory) { }

        [ScriptMethod(name: "Phase5 Initialization",
            eventType: EventTypeEnum.AddCombatant,
            eventCondition: ["DataId:17839"],
            userControl: false)]

        public void Phase5_Initialization_åˆå§‹åŒ–(Event @event, ScriptAccessory accessory)
        {

            phase5_bossId = @event["SourceId"];
            phase5_hasAcquiredTheFirstTower = false;
            phase5_indexOfTheFirstTower = "";
            phase5_hasConfirmedTheInitialPosition = false;

            System.Threading.Thread.MemoryBarrier();

            isInPhase5 = true;

        }

        [ScriptMethod(name: "Phase5 Destruction",
            eventType: EventTypeEnum.RemoveCombatant,
            eventCondition: ["DataId:17839"],
            userControl: false)]

        public void Phase5_Destruction_æžæž„(Event @event, ScriptAccessory accessory)
        {

            isInPhase5 = false;

            System.Threading.Thread.MemoryBarrier();

            phase5_bossId = "";
            phase5_hasAcquiredTheFirstTower = false;
            phase5_indexOfTheFirstTower = "";
            phase5_hasConfirmedTheInitialPosition = false;

        }

        [ScriptMethod(name: "P5_FulgentBlade", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40118|40307)$"])]
        public void P5_FulgentBlade(Event @event, ScriptAccessory accessory)
        {
            if (!isInPhase5)
            {

                return;

            }

            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_FulgentBlade";
            dp.Scale = new(80, 5);
            dp.Owner = sid;
            dp.Color = Phase5_Colour_Of_Fulgent_Blade.V4.WithW(1f);
            dp.DestoryAt = 7000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = $"P5_FulgentBlade_Advance_{@event["SourceId"]}";
            dp.Scale = new(80, 5);
            dp.Offset = new(0, 0, -5);
            dp.Owner = sid;
            dp.Color = Phase5_Colour_Of_Fulgent_Blade.V4.WithW(1f);
            dp.Delay = 7000;
            dp.DestoryAt = 20000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, dp);

        }
        [ScriptMethod(name: "P5_FulgentBladeClear", eventType: EventTypeEnum.ActionEffect, eventCondition: ["ActionId:regex:^(40118|4030[789])$"], userControl: false)]
        public void P5_FulgentBladeClear(Event @event, ScriptAccessory accessory)
        {
            if (!isInPhase5)
            {

                return;

            }

            if (!float.TryParse(@event["SourceRotation"], out var rot)) return;
            var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
            Vector3 centre = new(100, 0, 100);
            Vector3 posNext = new(pos.X + 5 * MathF.Sin(rot), 0, pos.Z + 5 * MathF.Cos(rot));
            if ((posNext - centre).Length() > 20)
            {
                accessory.Method.RemoveDraw($"P5_FulgentBlade_Advance_{@event["SourceId"]}");
            }
        }

        //1st fire glows effect
        [ScriptMethod(name: "Phase5 Guidance Of Fulgent Blade", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id2:16"])]
        public void Phase5_Guidance_Of_Fulgent_Blade_ç’€ç’¨ä¹‹åˆƒæŒ‡è·¯(Event @event, ScriptAccessory accessory)
        {
            if (Phase == "P5FulgentBladeCalcComplete")//restrict group
             {
                 lock (drawLock)
                 {
                     Phase = "P5CalcEnd";
                     //accessory.Method.SendChat($"/e P5CalcEnd");
                     var id = Convert.ToUInt32(@event["SourceId"], 16);
                     Vector2 FarthestPoint = new Vector2();
                     Vector2 ClosestPoint = new Vector2();
                     if (id == P1P3Blades[0].Id || id == P1P3Blades[1].Id) //P1 fire start
                     {
                         FarthestPoint = FindFarthestPoint(OnPoint, Point1);
                         ClosestPoint = FindClosestPoint(OnPoint, Point1);
                     }
                     else if (id == P1P3Blades[2].Id || id == P1P3Blades[3].Id) //P3 fire start
                     {
                         FarthestPoint = FindFarthestPoint(OnPoint, Point3);
                         ClosestPoint = FindClosestPoint(OnPoint, Point3);
                     }

                     //Far Near Near Far
                     BladeRoutes.Insert(0, FarthestPoint); //1st run start, farthest from fire point
                     BladeRoutes.Insert(1, ClosestPoint); //2nd run start, closest to fire point
                     BladeRoutes.Insert(2, FindFarthestPoint(OnPoint, Point2)); //3rd run path up or down, farthest from P2
                     BladeRoutes.Insert(3, FindClosestPoint(OnPoint, Point2)); //4th run path up or down, closest to P2
                     BladeRoutes.Insert(4, ClosestPoint); //5th run start, closest to fire point
                     BladeRoutes.Insert(5, FarthestPoint); //5th run start, farthest from fire point

                     //Initial guidance idea: 0 green 1 red, 1 green out 2 red, 2 green out 3 red
                     //2000ms each?
                     int BladeTimes = 2000;
                     //0 green 1 red
                     var Goline0 = accessory.Data.GetDefaultDrawProperties();
                     Goline0.Owner = accessory.Data.Me;
                     Goline0.DestoryAt = 9000;
                     Goline0.Color = Phase5_Colour_Of_The_Current_Guidance_Step.V4;
                     Goline0.Scale = new(2);
                     Goline0.ScaleMode |= ScaleMode.YByDistance;
                     Goline0.TargetPosition = Vector3Fucker(BladeRoutes[0]);
                     accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, Goline0);

                     var line1 = accessory.Data.GetDefaultDrawProperties();
                     line1.Position = Vector3Fucker(BladeRoutes[0]);
                     line1.DestoryAt = 9000;
                     line1.Color = Phase5_Colour_Of_The_Next_Guidance_Step.V4;
                     line1.Scale = new(2);
                     line1.ScaleMode |= ScaleMode.YByDistance;
                     line1.TargetPosition = Vector3Fucker(BladeRoutes[1]);
                     accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, line1);
                     /////////////////////////////////////1 green out 2 base delay 9000
                     var Goline1 = accessory.Data.GetDefaultDrawProperties();
                     Goline1.Owner = accessory.Data.Me;
                     Goline1.Delay = 9000;
                     Goline1.DestoryAt = BladeTimes;
                     Goline1.Color = Phase5_Colour_Of_The_Current_Guidance_Step.V4;
                     Goline1.Scale = new(2);
                     Goline1.ScaleMode |= ScaleMode.YByDistance;
                     Goline1.TargetPosition = Vector3Fucker(BladeRoutes[1]);
                     accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, Goline1);

                     var line2 = accessory.Data.GetDefaultDrawProperties();
                     line2.Position = Vector3Fucker(BladeRoutes[1]);
                     line2.Delay = 9000;
                     line2.DestoryAt = BladeTimes;
                     line2.Color = Phase5_Colour_Of_The_Next_Guidance_Step.V4;
                     line2.Scale = new(2);
                     line2.ScaleMode |= ScaleMode.YByDistance;
                     line2.TargetPosition = Vector3Fucker(BladeRoutes[2]);
                     accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, line2);

                     /////////////////////////////////////2 green out 3 base delay 9000+bladetime
                     var Goline2 = accessory.Data.GetDefaultDrawProperties();
                     Goline2.Owner = accessory.Data.Me;
                     Goline2.Delay = 9000 + BladeTimes;
                     Goline2.DestoryAt = BladeTimes;
                     Goline2.Color = Phase5_Colour_Of_The_Current_Guidance_Step.V4;
                     Goline2.Scale = new(2);
                     Goline2.ScaleMode |= ScaleMode.YByDistance;
                     Goline2.TargetPosition = Vector3Fucker(BladeRoutes[2]);
                     accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, Goline2);

                     var line3 = accessory.Data.GetDefaultDrawProperties();
                     line3.Position = Vector3Fucker(BladeRoutes[2]);
                     line3.Delay = 9000 + BladeTimes;
                     line3.DestoryAt = BladeTimes;
                     line3.Color = Phase5_Colour_Of_The_Next_Guidance_Step.V4;
                     line3.Scale = new(2);
                     line3.ScaleMode |= ScaleMode.YByDistance;
                     line3.TargetPosition = Vector3Fucker(BladeRoutes[3]);
                     accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, line3);

                     /////////////////////////////////////3 green out 4 base delay 9000+bladetime*2
                     var Goline3 = accessory.Data.GetDefaultDrawProperties();
                     Goline3.Owner = accessory.Data.Me;
                     Goline3.Delay = 9000 + BladeTimes * 2;
                     Goline3.DestoryAt = BladeTimes;
                     Goline3.Color = Phase5_Colour_Of_The_Current_Guidance_Step.V4;
                     Goline3.Scale = new(2);
                     Goline3.ScaleMode |= ScaleMode.YByDistance;
                     Goline3.TargetPosition = Vector3Fucker(BladeRoutes[3]);
                     accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, Goline3);

                     var line4 = accessory.Data.GetDefaultDrawProperties();
                     line4.Position = Vector3Fucker(BladeRoutes[3]);
                     line4.Delay = 9000 + BladeTimes * 2;
                     line4.DestoryAt = BladeTimes;
                     line4.Color = Phase5_Colour_Of_The_Next_Guidance_Step.V4;
                     line4.Scale = new(2);
                     line4.ScaleMode |= ScaleMode.YByDistance;
                     line4.TargetPosition = Vector3Fucker(BladeRoutes[4]);
                     accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, line4);

                     /////////////////////////////////////4 green out 5 base delay 9000+bladetime*3
                     var Goline4 = accessory.Data.GetDefaultDrawProperties();
                     Goline4.Owner = accessory.Data.Me;
                     Goline4.Delay = 9000 + BladeTimes * 3;
                     Goline4.DestoryAt = BladeTimes;
                     Goline4.Color = Phase5_Colour_Of_The_Current_Guidance_Step.V4;
                     Goline4.Scale = new(2);
                     Goline4.ScaleMode |= ScaleMode.YByDistance;
                     Goline4.TargetPosition = Vector3Fucker(BladeRoutes[4]);
                     accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, Goline4);

                     var line5 = accessory.Data.GetDefaultDrawProperties();
                     line5.Position = Vector3Fucker(BladeRoutes[4]);
                     line5.Delay = 9000 + BladeTimes * 3;
                     line5.DestoryAt = BladeTimes;
                     line5.Color = Phase5_Colour_Of_The_Next_Guidance_Step.V4;
                     line5.Scale = new(2);
                     line5.ScaleMode |= ScaleMode.YByDistance;
                     line5.TargetPosition = Vector3Fucker(BladeRoutes[5]);
                     accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, line5);

                     /////////////////////////////////////5 green out 6 base delay 9000+bladetime*4
                     var Goline5 = accessory.Data.GetDefaultDrawProperties();
                     Goline5.Owner = accessory.Data.Me;
                     Goline5.Delay = 9000 + BladeTimes * 4;
                     Goline5.DestoryAt = BladeTimes;
                     Goline5.Color = Phase5_Colour_Of_The_Current_Guidance_Step.V4;
                     Goline5.Scale = new(2);
                     Goline5.ScaleMode |= ScaleMode.YByDistance;
                     Goline5.TargetPosition = Vector3Fucker(BladeRoutes[5]);
                     accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, Goline5);
                 }
             }
        }

        [ScriptMethod(name: "Phase5 Boss Central Axis After Fulgent Blade",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40310"])]

        public void Phase5_Boss_Central_Axis_After_Fulgent_Blade_ç’€ç’¨ä¹‹åˆƒåŽBossä¸­è½´çº¿(Event @event, ScriptAccessory accessory)
        {

            if (!isInPhase5)
            {

                return;

            }

            if (!ParseObjectId(@event["SourceId"], out var sourceId))
            {

                return;

            }

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase5_Boss_Front_Axis_After_Fulgent_Blade";
            currentProperty.Owner = sourceId;
            currentProperty.Scale = new(0.5f, 10);
            currentProperty.Color = Phase5_Colour_Of_The_Boss_Central_Axis.V4.WithW(25f);
            currentProperty.DestoryAt = 9000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase5_Boss_Rear_Axis_After_Fulgent_Blade";
            currentProperty.Owner = sourceId;
            currentProperty.Scale = new(0.5f, 10);
            currentProperty.Rotation = float.Pi;
            currentProperty.Color = Phase5_Colour_Of_The_Boss_Central_Axis.V4.WithW(25f);
            currentProperty.DestoryAt = 9000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

        }

        [ScriptMethod(name: "Phase5 Side To Stack After Fulgent Blade",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40310"])]

        public void Phase5_Side_To_Stack_After_Fulgent_Blade_ç’€ç’¨ä¹‹åˆƒåŽçš„åˆ†æ‘Šä¾§(Event @event, ScriptAccessory accessory)
        {

            if (!isInPhase5)
            {

                return;

            }

            if (!ParseObjectId(@event["SourceId"], out var sourceId))
            {

                return;

            }

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);

            if (myIndex < 0 || myIndex > 7)
            {

                return;

            }

            bool goLeft = false;

            if (myIndex == 0
               ||
               myIndex == 2
               ||
               myIndex == 4
               ||
               myIndex == 6)
            {

                goLeft = true;

            }

            if (myIndex == 1
               ||
               myIndex == 3
               ||
               myIndex == 5
               ||
               myIndex == 7)
            {

                goLeft = false;

            }

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase5_Left_Side_After_Fulgent_Blade";
            currentProperty.Owner = sourceId;
            currentProperty.Scale = new(4);
            currentProperty.Radian = float.Pi;
            currentProperty.Rotation = float.Pi / 2;
            currentProperty.Offset = new Vector3(-0.25f, 0, 0);
            currentProperty.DestoryAt = 9000;

            if (Phase5_Boss_Faces_Players_After_Fulgent_Blade)
            {

                currentProperty.Color = goLeft ?
                    accessory.Data.DefaultDangerColor.WithW(25f) :
                    accessory.Data.DefaultSafeColor.WithW(25f);

            }

            else
            {

                currentProperty.Color = goLeft ?
                    accessory.Data.DefaultSafeColor.WithW(25f) :
                    accessory.Data.DefaultDangerColor.WithW(25f);

            }

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase5_Right_Side_After_Fulgent_Blade";
            currentProperty.Owner = sourceId;
            currentProperty.Scale = new(4);
            currentProperty.Radian = float.Pi;
            currentProperty.Rotation = -(float.Pi / 2);
            currentProperty.Offset = new Vector3(0.25f, 0, 0);
            currentProperty.DestoryAt = 9000;

            if (Phase5_Boss_Faces_Players_After_Fulgent_Blade)
            {

                currentProperty.Color = goLeft ?
                    accessory.Data.DefaultSafeColor.WithW(25f) :
                    accessory.Data.DefaultDangerColor.WithW(25f);

            }

            else
            {

                currentProperty.Color = goLeft ?
                    accessory.Data.DefaultDangerColor.WithW(25f) :
                    accessory.Data.DefaultSafeColor.WithW(25f);

            }

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, currentProperty);

            if (Enable_Text_Prompts)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    accessory.Method.TextInfo(((goLeft) ? ("å·¦ä¾§åˆ†æ‘Š") : ("å³ä¾§åˆ†æ‘Š")), 9000);

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    accessory.Method.TextInfo(((goLeft) ? ("Stack on the left") : ("Stack on the right")), 9000);

                }

            }

            if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    accessory.TTS($"{((goLeft) ? ("å·¦ä¾§åˆ†æ‘Š") : ("å³ä¾§åˆ†æ‘Š"))}",
                        Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    accessory.TTS($"{((goLeft) ? ("Stack on the left") : ("Stack on the right"))}",
                        Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

            }

        }

        [ScriptMethod(name: "Phase5 Initialization Of Wings Dark And Light",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40319"],
            userControl: false)]

        public void Phase5_Initialization_Of_Wings_Dark_And_Light_å…‰ä¸Žæš—ä¹‹ç¿¼åˆå§‹åŒ–(Event @event, ScriptAccessory accessory)
        {

            if (!isInPhase5)
            {

                return;

            }

            phase5_hasAcquiredTheFirstTower = false;
            phase5_indexOfTheFirstTower = "";
            phase5_hasConfirmedTheInitialPosition = false;

        }

        [ScriptMethod(name: "P5_WingsDarkAndLight", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:regex:^(40313|40233)$"])]
        public void P5_WingsDarkAndLight(Event @event, ScriptAccessory accessory)
        {
            if (!isInPhase5)
            {

                return;

            }

            if (!ParseObjectId(@event["SourceId"], out var sid)) return;

            var r = 225f;
            var rot = (180 - r / 2) / 180f * float.Pi;

            var dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_WingsDarkAndLight";
            dp.Scale = new(20);
            dp.Owner = sid;
            dp.Radian = r / 180 * float.Pi;
            dp.TargetObject = accessory.Data.EnmityList[sid][0];
            dp.Rotation = @event["ActionId"] == "40313" ? rot : -rot;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_WingsDarkAndLight_AwayClose";
            dp.Scale = new(4);
            dp.Owner = sid;
            dp.CentreResolvePattern = @event["ActionId"] == "40313" ? PositionResolvePatternEnum.PlayerFarestOrder : PositionResolvePatternEnum.PlayerNearestOrder;
            dp.Rotation = @event["ActionId"] == "40313" ? rot : -rot;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.DestoryAt = 7300;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_WingsDarkAndLight";
            dp.Scale = new(20);
            dp.Owner = sid;
            dp.Radian = r / 180 * float.Pi;
            dp.TargetResolvePattern = PositionResolvePatternEnum.OwnerTarget;
            dp.Rotation = @event["ActionId"] == "40313" ? -rot : rot;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7300;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Fan, dp);

            dp = accessory.Data.GetDefaultDrawProperties();
            dp.Name = "P5_WingsDarkAndLight_AwayClose";
            dp.Scale = new(4);
            dp.Owner = sid;
            dp.CentreResolvePattern = @event["ActionId"] == "40313" ? PositionResolvePatternEnum.PlayerNearestOrder : PositionResolvePatternEnum.PlayerFarestOrder;
            dp.Rotation = @event["ActionId"] == "40313" ? rot : -rot;
            dp.Color = accessory.Data.DefaultDangerColor;
            dp.Delay = 7300;
            dp.DestoryAt = 4000;
            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Circle, dp);
        }

        [ScriptMethod(name: "Phase5 Acquire The First Tower Of Wings Dark And Light",
            eventType: EventTypeEnum.EnvControl,
            eventCondition: ["DirectorId:800375BF", "State:00010004", "Index:regex:^(0000003[012])"],
            userControl: false)]

        public void Phase5_Acquire_The_First_Tower_Of_Wings_Dark_And_Light_èŽ·å–å…‰ä¸Žæš—ä¹‹ç¿¼ä¸€å¡”(Event @event, ScriptAccessory accessory)
        {

            if (!isInPhase5)
            {

                return;

            }

            if (!phase5_hasAcquiredTheFirstTower)
            {

                phase5_indexOfTheFirstTower = @event["Index"];

                phase5_hasAcquiredTheFirstTower = true;

            }

        }

        [ScriptMethod(name:"Phase5 Initial Position Of The Current MT Before Towers",
            eventType:EventTypeEnum.EnvControl,
            eventCondition:["DirectorId:800375BF","State:00010004","Index:regex:^(0000003[012])"])]

        public void Phase5_Initial_Position_Of_The_Current_MT_Before_Towers_è¸©å¡”å‰å½“å‰MTçš„èµ·å§‹ä½ç½®(Event @event, ScriptAccessory accessory) {

            if(!isInPhase5) {

                return;

            }

            if(phase5_hasConfirmedTheInitialPosition) {

                return;

            }

            else {

                phase5_hasConfirmedTheInitialPosition=true;

            }

            if(!ParseObjectId(phase5_bossId, out var bossId)) {

                return;

            }

            if(!accessory.Data.EnmityList.TryGetValue(bossId, out var enmityListOfBoss)) {

                return;

            }

            if(Enable_Developer_Mode) {

                accessory.Method.SendChat($"""
                                           /e 
                                           accessory.Data.Me={accessory.Data.Me}
                                           enmityListOfTheBoss[0]={enmityListOfBoss[0]}

                                           """);

            }

            if(accessory.Data.Me!=enmityListOfBoss[0]) {

                return;

            }

            while(!phase5_hasAcquiredTheFirstTower) {

                System.Threading.Thread.Sleep(1);

            }

            System.Threading.Thread.MemoryBarrier();

            Vector3 positionOfTheFirstTower=new Vector3(100,0,100);

            if(phase5_indexOfTheFirstTower.Equals("00000030")) {

                positionOfTheFirstTower=new Vector3(93.94f,0,96.50f);

            }

            if(phase5_indexOfTheFirstTower.Equals("00000031")) {

                positionOfTheFirstTower=new Vector3(106.06f,0,96.50f);

            }

            if(phase5_indexOfTheFirstTower.Equals("00000032")) {

                positionOfTheFirstTower=new Vector3(100f,0,107f);

            }

            if(positionOfTheFirstTower.Equals(new Vector3(100,0,100))) {

                return;

            }

            var currentProperty=accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name="Phase5_Initial_Position_Of_The_Current_MT_Before_Towers";
            currentProperty.Scale=new(2);
            currentProperty.Owner=accessory.Data.Me;
            currentProperty.TargetPosition=RotatePoint(positionOfTheFirstTower,new Vector3(100,0,100),float.Pi);
            currentProperty.ScaleMode|=ScaleMode.YByDistance;
            currentProperty.Color=accessory.Data.DefaultSafeColor;
            currentProperty.DestoryAt=2300;

            accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

        }

        [ScriptMethod(name:"Phase5 Guidance For Tanks During Towers",
            eventType:EventTypeEnum.StartCasting,
            eventCondition:["ActionId:regex:^(40313|40233)$"])]

        public void Phase5_Guidance_For_Tanks_During_Towers_å¦å…‹è¸©å¡”æŒ‡è·¯(Event @event, ScriptAccessory accessory) {

            if(!isInPhase5) {

                return;

            }

            if(!phase5_hasAcquiredTheFirstTower) {

                return;

            }

            if(accessory.Data.PartyList.IndexOf(accessory.Data.Me)!=0
               &&
               accessory.Data.PartyList.IndexOf(accessory.Data.Me)!=1) {

                return;

            }

            bool isCurrentMt=true;

            if(!ParseObjectId(phase5_bossId, out var bossId)) {

                return;

            }

            if(!accessory.Data.EnmityList.TryGetValue(bossId, out var enmityListOfBoss)) {

                return;

            }

            if(Enable_Developer_Mode) {

                accessory.Method.SendChat($"""
                                           /e 
                                           accessory.Data.Me={accessory.Data.Me}
                                           enmityListOfTheBoss[0]={enmityListOfBoss[0]}

                                           """);

            }

            if(accessory.Data.Me==enmityListOfBoss[0]) {

                isCurrentMt=true;

            }

            else {

                isCurrentMt=false;

            }

            bool isLeftFirstAndFarFirst=true;

            if(@event["ActionId"].Equals("40313")) {
                // 40313 stands for left first then right, far first then close.

                isLeftFirstAndFarFirst=true;

            }

            if(@event["ActionId"].Equals("40233")) {
                // 40233 stands for right first then left, close first then far.

                isLeftFirstAndFarFirst=false;

            }

            Vector3 positionOfTheFirstTower=new Vector3(100,0,100);

            if(phase5_indexOfTheFirstTower.Equals("00000030")) {

                positionOfTheFirstTower=new Vector3(93.94f,0,96.50f);

            }

            if(phase5_indexOfTheFirstTower.Equals("00000031")) {

                positionOfTheFirstTower=new Vector3(106.06f,0,96.50f);

            }

            if(phase5_indexOfTheFirstTower.Equals("00000032")) {

                positionOfTheFirstTower=new Vector3(100f,0,107f);

            }

            if(positionOfTheFirstTower.Equals(new Vector3(100,0,100))) {

                return;

            }

            if(Phase5_Strat_Of_Wings_Dark_And_Light==Phase5_Strats_Of_Wings_Dark_And_Light.Grey9_Brain_Dead_MT_First_Tower_Opposite_ç°ä¹è„‘æ­»æ³•MTä¸€å¡”å¯¹ä¾§_èŽ«çµå–µä¸ŽMMW) {

                Vector3 position1OfCurrentMt=RotatePoint(positionOfTheFirstTower,new Vector3(100,0,100),float.Pi);
                // Just opposite the first tower.
                Vector3 position2OfCurrentMt=(isLeftFirstAndFarFirst)?
                    (new((position1OfCurrentMt.X-100)/7+100,0,(position1OfCurrentMt.Z-100)/7+100)):
                    (new((position1OfCurrentMt.X-100)/7*18+100,0,(position1OfCurrentMt.Z-100)/7*18+100));
                // The calculations of Position 2 were directly inherited from Karlin's script.
                // I don't know the mathematical ideas behind the algorithm, but it works and it definitely works great.
                // So as a result, except the multiplier was adjusted from 15 to 18, I just keep the part as is.

                Vector3 position2OfCurrentOt=RotatePoint(position1OfCurrentMt,new(100,0,100),(isLeftFirstAndFarFirst)?
                                                                                                          (convertDegree(120f)):
                                                                                                          (convertDegree(-120f)));
                Vector3 position1OfCurrentOt=(isLeftFirstAndFarFirst)?
                    (new((position2OfCurrentOt.X-100)/7*18+100,0,(position2OfCurrentOt.Z-100)/7*18+100)):
                    (new((position2OfCurrentOt.X-100)/7+100,0,(position2OfCurrentOt.Z-100)/7+100));

                if(isCurrentMt) {

                    var currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Guidance_1_For_The_Current_MT_During_Towers";
                    currentProperty.Scale=new(2);
                    currentProperty.Owner=accessory.Data.Me;
                    currentProperty.TargetPosition=position1OfCurrentMt;
                    currentProperty.ScaleMode|=ScaleMode.YByDistance;
                    currentProperty.Color=accessory.Data.DefaultSafeColor;
                    currentProperty.DestoryAt=7150;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Guidance_2_Preview_For_The_Current_MT_During_Towers";
                    currentProperty.Scale=new(2);
                    currentProperty.Position=position1OfCurrentMt;
                    currentProperty.TargetPosition=position2OfCurrentMt;
                    currentProperty.ScaleMode|=ScaleMode.YByDistance;
                    currentProperty.Color=accessory.Data.DefaultSafeColor;
                    currentProperty.DestoryAt=7150;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Guidance_2_For_The_Current_MT_During_Towers";
                    currentProperty.Scale=new(2);
                    currentProperty.Owner=accessory.Data.Me;
                    currentProperty.TargetPosition=position2OfCurrentMt;
                    currentProperty.ScaleMode|=ScaleMode.YByDistance;
                    currentProperty.Color=accessory.Data.DefaultSafeColor;
                    currentProperty.Delay=7150;
                    currentProperty.DestoryAt=4250;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    if(Phase5_Reminder_To_Provoke_During_Wings_Dark_And_Light) {

                        System.Threading.Thread.Sleep(1500);

                        if(Enable_Text_Prompts) {

                            if(Language_Of_Prompts==Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡) {

                                accessory.Method.TextInfo("ç­‰å¾…æŒ‘è¡…åŽé€€é¿",2500);

                            }

                            if(Language_Of_Prompts==Languages_Of_Prompts.English_è‹±æ–‡) {

                                accessory.Method.TextInfo("Wait for provocation then shirk",2500);

                            }

                        }

                        if(Enable_Vanilla_TTS||Enable_Daily_Routines_TTS) {

                            if(Language_Of_Prompts==Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡) {

                                accessory.TTS("ç­‰å¾…æŒ‘è¡…åŽé€€é¿",Enable_Vanilla_TTS,Enable_Daily_Routines_TTS);

                            }

                            if(Language_Of_Prompts==Languages_Of_Prompts.English_è‹±æ–‡) {

                                accessory.TTS("Wait for provocation then shirk",Enable_Vanilla_TTS,Enable_Daily_Routines_TTS);

                            }

                        }

                    }

                }

                else {

                    var currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Guidance_1_For_The_Current_OT_During_Towers";
                    currentProperty.Scale=new(2);
                    currentProperty.Owner=accessory.Data.Me;
                    currentProperty.TargetPosition=position1OfCurrentOt;
                    currentProperty.ScaleMode|=ScaleMode.YByDistance;
                    currentProperty.Color=accessory.Data.DefaultSafeColor;
                    currentProperty.DestoryAt=7650;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Guidance_2_Preview_For_The_Current_OT_During_Towers";
                    currentProperty.Scale=new(2);
                    currentProperty.Position=position1OfCurrentOt;
                    currentProperty.TargetPosition=position2OfCurrentOt;
                    currentProperty.ScaleMode|=ScaleMode.YByDistance;
                    currentProperty.Color=accessory.Data.DefaultSafeColor;
                    currentProperty.DestoryAt=7650;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Guidance_2_For_The_Current_OT_During_Towers";
                    currentProperty.Scale=new(2);
                    currentProperty.Owner=accessory.Data.Me;
                    currentProperty.TargetPosition=position2OfCurrentOt;
                    currentProperty.ScaleMode|=ScaleMode.YByDistance;
                    currentProperty.Color=accessory.Data.DefaultSafeColor;
                    currentProperty.Delay=7650;
                    currentProperty.DestoryAt=3750;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    if(Phase5_Reminder_To_Provoke_During_Wings_Dark_And_Light) {

                        System.Threading.Thread.Sleep(1000);

                        if(Enable_Text_Prompts) {

                            if(Language_Of_Prompts==Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡) {

                                accessory.Method.TextInfo("ç«‹å³æŒ‘è¡…ï¼",2500);

                            }

                            if(Language_Of_Prompts==Languages_Of_Prompts.English_è‹±æ–‡) {

                                accessory.Method.TextInfo("Now provoke!",2500);

                            }

                        }

                        if(Enable_Vanilla_TTS||Enable_Daily_Routines_TTS) {

                            if(Language_Of_Prompts==Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡) {

                                accessory.TTS("ç«‹å³æŒ‘è¡…ï¼",Enable_Vanilla_TTS,Enable_Daily_Routines_TTS);

                            }

                            if(Language_Of_Prompts==Languages_Of_Prompts.English_è‹±æ–‡) {

                                accessory.TTS("Now provoke!",Enable_Vanilla_TTS,Enable_Daily_Routines_TTS);

                            }

                        }

                    }

                }

            }
            
            if(Phase5_Strat_Of_Wings_Dark_And_Light==Phase5_Strats_Of_Wings_Dark_And_Light.Reverse_Triangle_MT_Baits_In_Towers_å€’ä¸‰è§’æ³•MTåœ¨å¡”ä¸­å¼•å¯¼) {
                
                Vector3 positionOfTheLeftTower=RotatePoint(positionOfTheFirstTower,new Vector3(100,0,100),float.Pi/3*2);
                Vector3 positionOfTheRightTower=RotatePoint(positionOfTheFirstTower,new Vector3(100,0,100),-(float.Pi/3*2));
                Vector3 oppositeOfTheFirstTower=RotatePoint(positionOfTheFirstTower,new Vector3(100,0,100),float.Pi);

                Vector3 position1OfCurrentMt=(isLeftFirstAndFarFirst)?(positionOfTheRightTower):(positionOfTheLeftTower);
                // Always keep the first hit away from others.
                Vector3 position2OfCurrentMt=(isLeftFirstAndFarFirst)?
                    new Vector3((oppositeOfTheFirstTower.X-100)/7+100,0,(oppositeOfTheFirstTower.Z-100)/7+100):
                    new Vector3((position1OfCurrentMt.X-100)/7*18+100,0,(position1OfCurrentMt.Z-100)/7*18+100);

                Vector3 position2OfCurrentOt=(isLeftFirstAndFarFirst)?
                    (RotatePoint(positionOfTheLeftTower,new Vector3(100,0,100),float.Pi)):
                    (RotatePoint(positionOfTheRightTower,new Vector3(100,0,100),float.Pi));
                // OT would be opposite of the other tower.
                Vector3 position1OfCurrentOt=(isLeftFirstAndFarFirst)?
                    new Vector3((position2OfCurrentOt.X-100)/7*18+100,0,(position2OfCurrentOt.Z-100)/7*18+100):
                    new Vector3((positionOfTheFirstTower.X-100)/7+100,0,(positionOfTheFirstTower.Z-100)/7+100);

                if(isCurrentMt) {

                    var currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Guidance_1_For_The_Current_MT_During_Towers";
                    currentProperty.Scale=new(2);
                    currentProperty.Owner=accessory.Data.Me;
                    currentProperty.TargetPosition=position1OfCurrentMt;
                    currentProperty.ScaleMode|=ScaleMode.YByDistance;
                    currentProperty.Color=accessory.Data.DefaultSafeColor;
                    currentProperty.DestoryAt=7150;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Guidance_2_Preview_For_The_Current_MT_During_Towers";
                    currentProperty.Scale=new(2);
                    currentProperty.Position=position1OfCurrentMt;
                    currentProperty.TargetPosition=position2OfCurrentMt;
                    currentProperty.ScaleMode|=ScaleMode.YByDistance;
                    currentProperty.Color=accessory.Data.DefaultSafeColor;
                    currentProperty.DestoryAt=7150;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Guidance_2_For_The_Current_MT_During_Towers";
                    currentProperty.Scale=new(2);
                    currentProperty.Owner=accessory.Data.Me;
                    currentProperty.TargetPosition=position2OfCurrentMt;
                    currentProperty.ScaleMode|=ScaleMode.YByDistance;
                    currentProperty.Color=accessory.Data.DefaultSafeColor;
                    currentProperty.Delay=7150;
                    currentProperty.DestoryAt=4250;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    if(Phase5_Reminder_To_Provoke_During_Wings_Dark_And_Light) {

                        System.Threading.Thread.Sleep(1500);

                        if(Enable_Text_Prompts) {

                            if(Language_Of_Prompts==Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡) {

                                accessory.Method.TextInfo("ç­‰å¾…æŒ‘è¡…åŽé€€é¿",2500);

                            }

                            if(Language_Of_Prompts==Languages_Of_Prompts.English_è‹±æ–‡) {

                                accessory.Method.TextInfo("Wait for provocation then shirk",2500);

                            }

                        }

                        if(Enable_Vanilla_TTS||Enable_Daily_Routines_TTS) {

                            if(Language_Of_Prompts==Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡) {

                                accessory.TTS("ç­‰å¾…æŒ‘è¡…åŽé€€é¿",Enable_Vanilla_TTS,Enable_Daily_Routines_TTS);

                            }

                            if(Language_Of_Prompts==Languages_Of_Prompts.English_è‹±æ–‡) {

                                accessory.TTS("Wait for provocation then shirk",Enable_Vanilla_TTS,Enable_Daily_Routines_TTS);

                            }

                        }

                    }

                }

                else {

                    var currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Guidance_1_For_The_Current_OT_During_Towers";
                    currentProperty.Scale=new(2);
                    currentProperty.Owner=accessory.Data.Me;
                    currentProperty.TargetPosition=position1OfCurrentOt;
                    currentProperty.ScaleMode|=ScaleMode.YByDistance;
                    currentProperty.Color=accessory.Data.DefaultSafeColor;
                    currentProperty.DestoryAt=7650;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Guidance_2_Preview_For_The_Current_OT_During_Towers";
                    currentProperty.Scale=new(2);
                    currentProperty.Position=position1OfCurrentOt;
                    currentProperty.TargetPosition=position2OfCurrentOt;
                    currentProperty.ScaleMode|=ScaleMode.YByDistance;
                    currentProperty.Color=accessory.Data.DefaultSafeColor;
                    currentProperty.DestoryAt=7650;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Guidance_2_For_The_Current_OT_During_Towers";
                    currentProperty.Scale=new(2);
                    currentProperty.Owner=accessory.Data.Me;
                    currentProperty.TargetPosition=position2OfCurrentOt;
                    currentProperty.ScaleMode|=ScaleMode.YByDistance;
                    currentProperty.Color=accessory.Data.DefaultSafeColor;
                    currentProperty.Delay=7650;
                    currentProperty.DestoryAt=3750;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    if(Phase5_Reminder_To_Provoke_During_Wings_Dark_And_Light) {

                        System.Threading.Thread.Sleep(1000);

                        if(Enable_Text_Prompts) {

                            if (Language_Of_Prompts==Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡) {

                                accessory.Method.TextInfo("ç«‹å³æŒ‘è¡…ï¼",2500);

                            }

                            if(Language_Of_Prompts==Languages_Of_Prompts.English_è‹±æ–‡) {

                                accessory.Method.TextInfo("Now provoke!",2500);

                            }

                        }

                        if(Enable_Vanilla_TTS||Enable_Daily_Routines_TTS) {

                            if(Language_Of_Prompts==Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡) {

                                accessory.TTS("ç«‹å³æŒ‘è¡…ï¼",Enable_Vanilla_TTS,Enable_Daily_Routines_TTS);

                            }

                            if(Language_Of_Prompts==Languages_Of_Prompts.English_è‹±æ–‡) {

                                accessory.TTS("Now provoke!",Enable_Vanilla_TTS,Enable_Daily_Routines_TTS);

                            }

                        }

                    }

                }

            }

        }

        [ScriptMethod(name:"Phase5 Guidance For Others During Towers",
            eventType:EventTypeEnum.StartCasting,
            eventCondition:["ActionId:regex:^(40313|40233)$"])]

        public void Phase5_Guidance_For_Others_During_Towers_äººç¾¤è¸©å¡”æŒ‡è·¯(Event @event, ScriptAccessory accessory) {

            if(!isInPhase5) {

                return;

            }

            if(!phase5_hasAcquiredTheFirstTower) {

                return;

            }

            if(accessory.Data.PartyList.IndexOf(accessory.Data.Me)==0
               ||
               accessory.Data.PartyList.IndexOf(accessory.Data.Me)==1) {

                return;

            }

            bool isLeftFirstAndFarFirst=true;

            if(@event["ActionId"].Equals("40313")) {

                isLeftFirstAndFarFirst=true;

            }

            if(@event["ActionId"].Equals("40233")) {

                isLeftFirstAndFarFirst=false;

            }

            /*
            if (Phase5_Strat_Of_Wings_Dark_And_Light == Phase5_Strats_Of_Wings_Dark_And_Light.Grey9_Brain_Dead_MT_First_Tower_Opposite_ç°ä¹è„‘æ­»æ³•MTä¸€å¡”å¯¹ä¾§_èŽ«çµå–µä¸ŽMMW)
            {
                // ... large commented-out original code ... (kept as is in the original file)
            }
            */
            
            if(Phase5_Strat_Of_Wings_Dark_And_Light==Phase5_Strats_Of_Wings_Dark_And_Light.Grey9_Brain_Dead_MT_First_Tower_Opposite_ç°ä¹è„‘æ­»æ³•MTä¸€å¡”å¯¹ä¾§_èŽ«çµå–µä¸ŽMMW) {
                // The previous spaghetti code is commented out. The entire part was reworked.

                float rotation=0;

                if(phase5_indexOfTheFirstTower.Equals("00000030")) {

                    rotation=float.Pi/3*2;

                }

                if(phase5_indexOfTheFirstTower.Equals("00000031")) {
                    
                    rotation=-(float.Pi/3*2);

                }

                if(phase5_indexOfTheFirstTower.Equals("00000032")) {

                    rotation=0;

                }

                Vector3 positionOfTheFirstTower=RotatePoint(new Vector3(100,0,107),new Vector3(100,0,100),rotation);
                Vector3 positionOfTheLeftTower=RotatePoint(positionOfTheFirstTower,new Vector3(100,0,100),float.Pi/3*2);
                Vector3 positionOfTheRightTower=RotatePoint(positionOfTheFirstTower,new Vector3(100,0,100),-(float.Pi/3*2));
                Vector3 leftOfTheFirstTower=RotatePoint(phase5_leftSideOfTheSouth_asAConstant,new Vector3(100,0,100),rotation);
                Vector3 rightOfTheFirstTower=RotatePoint(phase5_rightSideOfTheSouth_asAConstant,new Vector3(100,0,100),rotation);
                Vector3 leftOfTheLeftTower=RotatePoint(phase5_leftSideOfTheNorthwest_asAConstant,new Vector3(100,0,100),rotation);
                Vector3 rightOfTheRightTower=RotatePoint(phase5_rightSideOfTheNortheast_asAConstant,new Vector3(100,0,100),rotation);
                Vector3 oppositeStandbyPosition=RotatePoint(positionOfTheFirstTower,new Vector3(100,0,100),float.Pi);
                Vector3 leftStandbyPosition=RotatePoint(phase5_standbyPointBetweenSouthAndNorthwest_asAConstant,new Vector3(100,0,100),rotation);
                Vector3 rightStandbyPosition=RotatePoint(phase5_standbyPointBetweenSouthAndNortheast_asAConstant,new Vector3(100,0,100),rotation);
                var currentProperty=accessory.Data.GetDefaultDrawProperties();
                
                if(Phase5_Branch_Of_The_Grey9_Brain_Dead_Strat==Phase5_Branches_Of_The_Grey9_Brain_Dead_Strat.Melees_First_Then_Healers_Left_Ranges_Right_è¿‘æˆ˜å…ˆç„¶åŽå¥¶å¦ˆå·¦è¿œç¨‹å³) {

                    bool isMelee=false;
                    bool isRange=false;
                    bool isHealer=false;
                    
                    if(accessory.Data.PartyList.IndexOf(accessory.Data.Me)==4
                       ||
                       accessory.Data.PartyList.IndexOf(accessory.Data.Me)==5) {

                        isMelee=true;

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        
                        currentProperty.Name="Phase5_Guidance_1_For_Melees_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=(isLeftFirstAndFarFirst)?(rightOfTheFirstTower):(leftOfTheFirstTower);
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=7300;
                        
                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_Preview_For_Melees_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Position=(isLeftFirstAndFarFirst)?(rightOfTheFirstTower):(leftOfTheFirstTower);
                        currentProperty.TargetPosition=oppositeStandbyPosition;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=7300;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_For_Melees_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=oppositeStandbyPosition;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.Delay=7300;
                        currentProperty.DestoryAt=7100;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    }

                    if(accessory.Data.PartyList.IndexOf(accessory.Data.Me)==6
                       ||
                       accessory.Data.PartyList.IndexOf(accessory.Data.Me)==7) {

                        isRange=true;
                        
                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_1_For_Ranges_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=(isLeftFirstAndFarFirst)?(rightStandbyPosition):(leftStandbyPosition);
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_Preview_For_Ranges_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Position=(isLeftFirstAndFarFirst)?(rightStandbyPosition):(leftStandbyPosition);
                        currentProperty.TargetPosition=rightOfTheRightTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_For_Ranges_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=rightOfTheRightTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.Delay=6900;
                        currentProperty.DestoryAt=7500;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    }

                    if(accessory.Data.PartyList.IndexOf(accessory.Data.Me)==2
                       ||
                       accessory.Data.PartyList.IndexOf(accessory.Data.Me)==3) {

                        isHealer=true;
                        
                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_1_For_Healers_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=(isLeftFirstAndFarFirst)?(rightStandbyPosition):(leftStandbyPosition);
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor; 
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_Preview_For_Healers_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Position=(isLeftFirstAndFarFirst)?(rightStandbyPosition):(leftStandbyPosition);
                        currentProperty.TargetPosition=leftOfTheLeftTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_For_Healers_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=leftOfTheLeftTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.Delay=6900;
                        currentProperty.DestoryAt=7500;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);
                        
                    }
                    
                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Range_Of_Towers";
                    currentProperty.Scale=new(3);
                    currentProperty.Position=positionOfTheFirstTower;
                    currentProperty.Color=(isMelee)?(accessory.Data.DefaultSafeColor):(accessory.Data.DefaultDangerColor);
                    currentProperty.DestoryAt=7300;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Circle,currentProperty);
                    
                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Range_Of_Towers";
                    currentProperty.Scale=new(3);
                    currentProperty.Position=positionOfTheLeftTower;
                    currentProperty.Color=(isHealer)?(accessory.Data.DefaultSafeColor):(accessory.Data.DefaultDangerColor);
                    currentProperty.DestoryAt=14400;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Circle,currentProperty);
                    
                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Range_Of_Towers";
                    currentProperty.Scale=new(3);
                    currentProperty.Position=positionOfTheRightTower;
                    currentProperty.Color=(isRange)?(accessory.Data.DefaultSafeColor):(accessory.Data.DefaultDangerColor);
                    currentProperty.DestoryAt=14400;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Circle,currentProperty);

                }

                if(Phase5_Branch_Of_The_Grey9_Brain_Dead_Strat==Phase5_Branches_Of_The_Grey9_Brain_Dead_Strat.Healers_First_Then_Melees_Left_Ranges_Right_å¥¶å¦ˆå…ˆç„¶åŽè¿‘æˆ˜å·¦è¿œç¨‹å³_èŽ«çµå–µ) {
                    
                    bool isMelee=false;
                    bool isRange=false;
                    bool isHealer=false;
                    
                    if(accessory.Data.PartyList.IndexOf(accessory.Data.Me)==2
                       ||
                       accessory.Data.PartyList.IndexOf(accessory.Data.Me)==3) {

                        isHealer=true;

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        
                        currentProperty.Name="Phase5_Guidance_1_For_Healers_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=(isLeftFirstAndFarFirst)?(rightOfTheFirstTower):(leftOfTheFirstTower);
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=7300;
                        
                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_Preview_For_Healers_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Position=(isLeftFirstAndFarFirst)?(rightOfTheFirstTower):(leftOfTheFirstTower);
                        currentProperty.TargetPosition=oppositeStandbyPosition;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=7300;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_For_Healers_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=oppositeStandbyPosition;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.Delay=7300;
                        currentProperty.DestoryAt=7100;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    }

                    if(accessory.Data.PartyList.IndexOf(accessory.Data.Me)==6
                       ||
                       accessory.Data.PartyList.IndexOf(accessory.Data.Me)==7) {

                        isRange=true;
                        
                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_1_For_Ranges_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=(isLeftFirstAndFarFirst)?(rightStandbyPosition):(leftStandbyPosition);
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_Preview_For_Ranges_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Position=(isLeftFirstAndFarFirst)?(rightStandbyPosition):(leftStandbyPosition);
                        currentProperty.TargetPosition=rightOfTheRightTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_For_Ranges_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=rightOfTheRightTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.Delay=6900;
                        currentProperty.DestoryAt=7500;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    }

                    if(accessory.Data.PartyList.IndexOf(accessory.Data.Me)==4
                       ||
                       accessory.Data.PartyList.IndexOf(accessory.Data.Me)==5) {

                        isMelee=true;
                        
                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_1_For_Melees_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=(isLeftFirstAndFarFirst)?(rightStandbyPosition):(leftStandbyPosition);
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor; 
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_Preview_For_Melees_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Position=(isLeftFirstAndFarFirst)?(rightStandbyPosition):(leftStandbyPosition);
                        currentProperty.TargetPosition=leftOfTheLeftTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_For_Melees_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=leftOfTheLeftTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.Delay=6900;
                        currentProperty.DestoryAt=7500;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);
                        
                    }
                    
                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Range_Of_Towers";
                    currentProperty.Scale=new(3);
                    currentProperty.Position=positionOfTheFirstTower;
                    currentProperty.Color=(isHealer)?(accessory.Data.DefaultSafeColor):(accessory.Data.DefaultDangerColor);
                    currentProperty.DestoryAt=7300;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Circle,currentProperty);
                    
                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Range_Of_Towers";
                    currentProperty.Scale=new(3);
                    currentProperty.Position=positionOfTheLeftTower;
                    currentProperty.Color=(isMelee)?(accessory.Data.DefaultSafeColor):(accessory.Data.DefaultDangerColor);
                    currentProperty.DestoryAt=14400;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Circle,currentProperty);
                    
                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Range_Of_Towers";
                    currentProperty.Scale=new(3);
                    currentProperty.Position=positionOfTheRightTower;
                    currentProperty.Color=(isRange)?(accessory.Data.DefaultSafeColor):(accessory.Data.DefaultDangerColor);
                    currentProperty.DestoryAt=14400;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Circle,currentProperty);
                    
                }

                if(Phase5_Branch_Of_The_Grey9_Brain_Dead_Strat==Phase5_Branches_Of_The_Grey9_Brain_Dead_Strat.Healer_First_Then_Melees_Farther_Ranges_Closer_å¥¶å¦ˆå…ˆç„¶åŽè¿‘æˆ˜è¿œè¿œç¨‹è¿‘_MMW) {
                    
                    Vector3 positionOfTheCloserTower=(isLeftFirstAndFarFirst)?(positionOfTheRightTower):(positionOfTheLeftTower);
                    Vector3 positionOfTheFartherTower=(isLeftFirstAndFarFirst)?(positionOfTheLeftTower):(positionOfTheRightTower);
                    Vector3 positionToTakeTheCloserTower=(isLeftFirstAndFarFirst)?(rightOfTheRightTower):(leftOfTheLeftTower);
                    Vector3 positionToTakeTheFartherTower=(isLeftFirstAndFarFirst)?(leftOfTheLeftTower):(rightOfTheRightTower);
                    
                    bool isMelee=false;
                    bool isRange=false;
                    bool isHealer=false;
                    
                    if(accessory.Data.PartyList.IndexOf(accessory.Data.Me)==2
                       ||
                       accessory.Data.PartyList.IndexOf(accessory.Data.Me)==3) {

                        isHealer=true;

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        
                        currentProperty.Name="Phase5_Guidance_1_For_Healers_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=(isLeftFirstAndFarFirst)?(rightOfTheFirstTower):(leftOfTheFirstTower);
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=7300;
                        
                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_Preview_For_Healers_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Position=(isLeftFirstAndFarFirst)?(rightOfTheFirstTower):(leftOfTheFirstTower);
                        currentProperty.TargetPosition=oppositeStandbyPosition;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=7300;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_For_Healers_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=oppositeStandbyPosition;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.Delay=7300;
                        currentProperty.DestoryAt=7100;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    }

                    if(accessory.Data.PartyList.IndexOf(accessory.Data.Me)==6
                       ||
                       accessory.Data.PartyList.IndexOf(accessory.Data.Me)==7) {

                        isRange=true;
                        
                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_1_For_Ranges_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=(isLeftFirstAndFarFirst)?(rightStandbyPosition):(leftStandbyPosition);
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_Preview_For_Ranges_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Position=(isLeftFirstAndFarFirst)?(rightStandbyPosition):(leftStandbyPosition);
                        currentProperty.TargetPosition=positionToTakeTheCloserTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_For_Ranges_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=positionToTakeTheCloserTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.Delay=6900;
                        currentProperty.DestoryAt=7500;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    }

                    if(accessory.Data.PartyList.IndexOf(accessory.Data.Me)==4
                       ||
                       accessory.Data.PartyList.IndexOf(accessory.Data.Me)==5) {

                        isMelee=true;
                        
                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_1_For_Melees_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=(isLeftFirstAndFarFirst)?(rightStandbyPosition):(leftStandbyPosition);
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor; 
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_Preview_For_Melees_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Position=(isLeftFirstAndFarFirst)?(rightStandbyPosition):(leftStandbyPosition);
                        currentProperty.TargetPosition=positionToTakeTheFartherTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_For_Melees_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=positionToTakeTheFartherTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.Delay=6900;
                        currentProperty.DestoryAt=7500;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);
                        
                    }
                    
                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Range_Of_Towers";
                    currentProperty.Scale=new(3);
                    currentProperty.Position=positionOfTheFirstTower;
                    currentProperty.Color=(isHealer)?(accessory.Data.DefaultSafeColor):(accessory.Data.DefaultDangerColor);
                    currentProperty.DestoryAt=7300;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Circle,currentProperty);
                    
                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Range_Of_Towers";
                    currentProperty.Scale=new(3);
                    currentProperty.Position=positionOfTheFartherTower;
                    currentProperty.Color=(isMelee)?(accessory.Data.DefaultSafeColor):(accessory.Data.DefaultDangerColor);
                    currentProperty.DestoryAt=14400;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Circle,currentProperty);
                    
                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Range_Of_Towers";
                    currentProperty.Scale=new(3);
                    currentProperty.Position=positionOfTheCloserTower;
                    currentProperty.Color=(isRange)?(accessory.Data.DefaultSafeColor):(accessory.Data.DefaultDangerColor);
                    currentProperty.DestoryAt=14400;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Circle,currentProperty);
                    
                }

            }
            
            if(Phase5_Strat_Of_Wings_Dark_And_Light==Phase5_Strats_Of_Wings_Dark_And_Light.Reverse_Triangle_MT_Baits_In_Towers_å€’ä¸‰è§’æ³•MTåœ¨å¡”ä¸­å¼•å¯¼) {

                float rotation=0;

                if(phase5_indexOfTheFirstTower.Equals("00000030")) {

                    rotation=float.Pi/3*2;

                }

                if(phase5_indexOfTheFirstTower.Equals("00000031")) {
                    
                    rotation=-(float.Pi/3*2);

                }

                if(phase5_indexOfTheFirstTower.Equals("00000032")) {

                    rotation=0;

                }

                Vector3 positionOfTheFirstTower=RotatePoint(new Vector3(100,0,107),new Vector3(100,0,100),rotation);
                Vector3 positionOfTheLeftTower=RotatePoint(positionOfTheFirstTower,new Vector3(100,0,100),float.Pi/3*2);
                Vector3 positionOfTheRightTower=RotatePoint(positionOfTheFirstTower,new Vector3(100,0,100),-(float.Pi/3*2));
                Vector3 leftOfTheLeftTower=RotatePoint(phase5_leftSideOfTheNorthwest_asAConstant,new Vector3(100,0,100),rotation);
                Vector3 rightOfTheRightTower=RotatePoint(phase5_rightSideOfTheNortheast_asAConstant,new Vector3(100,0,100),rotation);
                Vector3 oppositeStandbyPosition=RotatePoint(positionOfTheFirstTower,new Vector3(100,0,100),float.Pi);
                Vector3 leftStandbyPosition=RotatePoint(phase5_standbyPointBetweenSouthAndNorthwest_asAConstant,new Vector3(100,0,100),rotation-(float.Pi/12));
                Vector3 rightStandbyPosition=RotatePoint(phase5_standbyPointBetweenSouthAndNortheast_asAConstant,new Vector3(100,0,100),rotation+(float.Pi/12));
                // I won't be so stupid as to enumerate every position anymore.
                // Maybe it could say that this is some kind of my growth?
                var currentProperty=accessory.Data.GetDefaultDrawProperties();
                
                if(Phase5_Branch_Of_The_Reverse_Triangle_Strat==Phase5_Branches_Of_The_Reverse_Triangle_Strat.Melees_First_Then_Healers_Left_Ranges_Right_è¿‘æˆ˜å…ˆç„¶åŽå¥¶å¦ˆå·¦è¿œç¨‹å³) {

                    bool isMelee=false;
                    bool isRange=false;
                    bool isHealer=false;
                    
                    if(accessory.Data.PartyList.IndexOf(accessory.Data.Me)==4
                       ||
                       accessory.Data.PartyList.IndexOf(accessory.Data.Me)==5) {

                        isMelee=true;

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        
                        currentProperty.Name="Phase5_Guidance_1_For_Melees_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=positionOfTheFirstTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=7300;
                        
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_Preview_For_Melees_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Position=positionOfTheFirstTower;
                        currentProperty.TargetPosition=oppositeStandbyPosition;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=7300;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_For_Melees_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=oppositeStandbyPosition;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.Delay=7300;
                        currentProperty.DestoryAt=7100;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                    }

                    if(accessory.Data.PartyList.IndexOf(accessory.Data.Me)==6
                       ||
                       accessory.Data.PartyList.IndexOf(accessory.Data.Me)==7) {

                        isRange=true;
                        
                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_1_For_Ranges_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=rightStandbyPosition;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_Preview_For_Ranges_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Position=rightStandbyPosition;
                        currentProperty.TargetPosition=rightOfTheRightTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_For_Ranges_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=rightOfTheRightTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.Delay=6900;
                        currentProperty.DestoryAt=7500;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    }

                    if(accessory.Data.PartyList.IndexOf(accessory.Data.Me)==2
                       ||
                       accessory.Data.PartyList.IndexOf(accessory.Data.Me)==3) {

                        isHealer=true;
                        
                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_1_For_Healers_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=leftStandbyPosition;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor; 
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_Preview_For_Healers_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Position=leftStandbyPosition;
                        currentProperty.TargetPosition=leftOfTheLeftTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_For_Healers_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=leftOfTheLeftTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.Delay=6900;
                        currentProperty.DestoryAt=7500;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);
                        
                    }
                    
                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Range_Of_Towers";
                    currentProperty.Scale=new(3);
                    currentProperty.Position=positionOfTheFirstTower;
                    currentProperty.Color=(isMelee)?(accessory.Data.DefaultSafeColor):(accessory.Data.DefaultDangerColor);
                    currentProperty.DestoryAt=7300;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Circle,currentProperty);
                    
                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Range_Of_Towers";
                    currentProperty.Scale=new(3);
                    currentProperty.Position=positionOfTheLeftTower;
                    currentProperty.Color=(isHealer)?(accessory.Data.DefaultSafeColor):(accessory.Data.DefaultDangerColor);
                    currentProperty.DestoryAt=14400;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Circle,currentProperty);
                    
                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Range_Of_Towers";
                    currentProperty.Scale=new(3);
                    currentProperty.Position=positionOfTheRightTower;
                    currentProperty.Color=(isRange)?(accessory.Data.DefaultSafeColor):(accessory.Data.DefaultDangerColor);
                    currentProperty.DestoryAt=14400;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Circle,currentProperty);

                }

                if(Phase5_Branch_Of_The_Reverse_Triangle_Strat==Phase5_Branches_Of_The_Reverse_Triangle_Strat.Healers_First_Then_Melees_Left_Ranges_Right_å¥¶å¦ˆå…ˆç„¶åŽè¿‘æˆ˜å·¦è¿œç¨‹å³) {
                    
                    bool isMelee=false;
                    bool isRange=false;
                    bool isHealer=false;
                    
                    if(accessory.Data.PartyList.IndexOf(accessory.Data.Me)==2
                       ||
                       accessory.Data.PartyList.IndexOf(accessory.Data.Me)==3) {

                        isHealer=true;

                        currentProperty = accessory.Data.GetDefaultDrawProperties();
                        
                        currentProperty.Name="Phase5_Guidance_1_For_Healers_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=positionOfTheFirstTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=7300;
                        
                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_Preview_For_Healers_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Position=positionOfTheFirstTower;
                        currentProperty.TargetPosition=oppositeStandbyPosition;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=7300;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_For_Healers_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=oppositeStandbyPosition;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.Delay=7300;
                        currentProperty.DestoryAt=7100;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                    }

                    if(accessory.Data.PartyList.IndexOf(accessory.Data.Me)==6
                       ||
                       accessory.Data.PartyList.IndexOf(accessory.Data.Me)==7) {

                        isRange=true;
                        
                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_1_For_Ranges_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=rightStandbyPosition;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_Preview_For_Ranges_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Position=rightStandbyPosition;
                        currentProperty.TargetPosition=rightOfTheRightTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_For_Ranges_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=rightOfTheRightTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.Delay=6900;
                        currentProperty.DestoryAt=7500;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                    }

                    if(accessory.Data.PartyList.IndexOf(accessory.Data.Me)==4
                       ||
                       accessory.Data.PartyList.IndexOf(accessory.Data.Me)==5) {

                        isMelee=true;
                        
                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_1_For_Melees_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=leftStandbyPosition;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor; 
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_Preview_For_Melees_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Position=leftStandbyPosition;
                        currentProperty.TargetPosition=leftOfTheLeftTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.DestoryAt=6900;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);

                        currentProperty=accessory.Data.GetDefaultDrawProperties();

                        currentProperty.Name="Phase5_Guidance_2_For_Melees_During_Towers";
                        currentProperty.Scale=new(2);
                        currentProperty.Owner=accessory.Data.Me;
                        currentProperty.TargetPosition=leftOfTheLeftTower;
                        currentProperty.ScaleMode|=ScaleMode.YByDistance;
                        currentProperty.Color=accessory.Data.DefaultSafeColor;
                        currentProperty.Delay=6900;
                        currentProperty.DestoryAt=7500;

                        accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Displacement,currentProperty);
                        
                    }
                    
                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Range_Of_Towers";
                    currentProperty.Scale=new(3);
                    currentProperty.Position=positionOfTheFirstTower;
                    currentProperty.Color=(isHealer)?(accessory.Data.DefaultSafeColor):(accessory.Data.DefaultDangerColor);
                    currentProperty.DestoryAt=7300;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Circle,currentProperty);
                    
                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Range_Of_Towers";
                    currentProperty.Scale=new(3);
                    currentProperty.Position=positionOfTheLeftTower;
                    currentProperty.Color=(isMelee)?(accessory.Data.DefaultSafeColor):(accessory.Data.DefaultDangerColor);
                    currentProperty.DestoryAt=14400;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Circle,currentProperty);
                    
                    currentProperty=accessory.Data.GetDefaultDrawProperties();
                    
                    currentProperty.Name="Phase5_Range_Of_Towers";
                    currentProperty.Scale=new(3);
                    currentProperty.Position=positionOfTheRightTower;
                    currentProperty.Color=(isRange)?(accessory.Data.DefaultSafeColor):(accessory.Data.DefaultDangerColor);
                    currentProperty.DestoryAt=14400;
                    
                    accessory.Method.SendDraw(DrawModeEnum.Imgui,DrawTypeEnum.Circle,currentProperty);
                    
                }

            }

        }

        [ScriptMethod(name: "Phase5 Boss Central Axis During Polarizing Strikes",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40316"])]

        public void Phase5_Boss_Central_Axis_During_Polarizing_Strikes_æžåŒ–æ‰“å‡»æœŸé—´Bossä¸­è½´çº¿(Event @event, ScriptAccessory accessory)
        {

            if (!isInPhase5)
            {

                return;

            }

            if (!ParseObjectId(@event["SourceId"], out var sourceId))
            {

                return;

            }

            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase5_Boss_Front_Axis_During_Polarizing_Strikes";
            currentProperty.Owner = sourceId;
            currentProperty.Scale = new(0.5f, 10);
            currentProperty.Color = Phase5_Colour_Of_The_Boss_Central_Axis.V4.WithW(25f);
            currentProperty.DestoryAt = 24000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase5_Boss_Rear_Axis_During_Polarizing_Strikes";
            currentProperty.Owner = sourceId;
            currentProperty.Scale = new(0.5f, 10);
            currentProperty.Rotation = float.Pi;
            currentProperty.Color = Phase5_Colour_Of_The_Boss_Central_Axis.V4.WithW(25f);
            currentProperty.DestoryAt = 24000;

            accessory.Method.SendDraw(DrawModeEnum.Default, DrawTypeEnum.Rect, currentProperty);

        }

        [ScriptMethod(name: "Phase5 Guidance Of Polarizing Strikes",
            eventType: EventTypeEnum.StartCasting,
            eventCondition: ["ActionId:40316"])]

        public void Phase5_Guidance_Of_Polarizing_Strikes_æžåŒ–æ‰“å‡»æŒ‡è·¯(Event @event, ScriptAccessory accessory)
        {

            if (!isInPhase5)
            {

                return;

            }

            if (!float.TryParse(@event["SourceRotation"], out float currentRotation))
            {

                return;

            }

            currentRotation = -(currentRotation - float.Pi);

            if (Enable_Developer_Mode)
            {

                accessory.Method.SendChat($"""
                                           /e 
                                           currentRotation={currentRotation}

                                           """);

            }

            int myIndex = accessory.Data.PartyList.IndexOf(accessory.Data.Me);
            int myRoundToTakeHits = phase5_getRoundToTakeHits(myIndex);
            bool inTheLeftGroup = true;
            int timelineControl = 0;
            int timeToTakeHits = 0;
            var currentProperty = accessory.Data.GetDefaultDrawProperties();

            if (myRoundToTakeHits < 1 || myRoundToTakeHits > 4)
            {

                return;

            }

            if (myIndex == 0
               ||
               myIndex == 2
               ||
               myIndex == 4
               ||
               myIndex == 6)
            {

                inTheLeftGroup = true;

            }

            if (myIndex == 1
               ||
               myIndex == 3
               ||
               myIndex == 5
               ||
               myIndex == 7)
            {

                inTheLeftGroup = false;

            }

            // ----- Initial guidance -----

            if (myRoundToTakeHits == 1)
            {

                currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase5_Initial_Guidance_Of_Polarizing_Strikes";
                currentProperty.Scale = new(2);
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = inTheLeftGroup ?
                    RotatePoint(phase5_positionToTakeHitsOnTheLeft_asAConstant, new Vector3(100, 0, 100), currentRotation) :
                    RotatePoint(phase5_positionToTakeHitsOnTheRight_asAConstant, new Vector3(100, 0, 100), currentRotation);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.DestoryAt = 4550;
                timelineControl += 4550;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            }

            else
            {

                currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase5_Initial_Guidance_Of_Polarizing_Strikes";
                currentProperty.Scale = new(2);
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = inTheLeftGroup ?
                    RotatePoint(phase5_positionToBeCoveredOnTheLeft_asAConstant, new Vector3(100, 0, 100), currentRotation) :
                    RotatePoint(phase5_positionToBeCoveredOnTheRight_asAConstant, new Vector3(100, 0, 100), currentRotation);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.DestoryAt = 4550;
                timelineControl += 4550;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            }

            // ----- Be covered in the current group -----

            for (int i = 1; i < myRoundToTakeHits; ++i)
            {

                currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase5_Inward_Guidance_Of_Polarizing_Strikes_In_The_Current_Group";
                currentProperty.Scale = new(2);
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = inTheLeftGroup ?
                    RotatePoint(phase5_positionToBeCoveredOnTheLeft_asAConstant, new Vector3(100, 0, 100), currentRotation) :
                    RotatePoint(phase5_positionToBeCoveredOnTheRight_asAConstant, new Vector3(100, 0, 100), currentRotation);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.Delay = timelineControl;
                currentProperty.DestoryAt = 2450;
                timelineControl += 2450;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase5_Outward_Guidance_Of_Polarizing_Strikes_In_The_Current_Group";
                currentProperty.Scale = new(2);
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = inTheLeftGroup ?
                    RotatePoint(phase5_positionToStandbyOnTheLeft_asAConstant, new Vector3(100, 0, 100), currentRotation) :
                    RotatePoint(phase5_positionToStandbyOnTheRight_asAConstant, new Vector3(100, 0, 100), currentRotation);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.Delay = timelineControl;
                currentProperty.DestoryAt = 2250;
                timelineControl += 2250;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            }

            // ----- -----

            // ----- Take hits and swap the group -----

            timeToTakeHits = timelineControl - 250;

            currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase5_Inward_Guidance_Of_Polarizing_Strikes_While_Taking_Hits";
            currentProperty.Scale = new(2);
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.TargetPosition = inTheLeftGroup ?
                RotatePoint(phase5_positionToTakeHitsOnTheLeft_asAConstant, new Vector3(100, 0, 100), currentRotation) :
                RotatePoint(phase5_positionToTakeHitsOnTheRight_asAConstant, new Vector3(100, 0, 100), currentRotation);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.Delay = timelineControl;
            currentProperty.DestoryAt = 2450;
            timelineControl += 2450;

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            currentProperty = accessory.Data.GetDefaultDrawProperties();

            currentProperty.Name = "Phase5_Outward_Guidance_Of_Polarizing_Strikes_While_Taking_Hits";
            currentProperty.Scale = new(2);
            currentProperty.Owner = accessory.Data.Me;
            currentProperty.TargetPosition = inTheLeftGroup ?
                RotatePoint(phase5_positionToStandbyOnTheRight_asAConstant, new Vector3(100, 0, 100), currentRotation) :
                RotatePoint(phase5_positionToStandbyOnTheLeft_asAConstant, new Vector3(100, 0, 100), currentRotation);
            currentProperty.ScaleMode |= ScaleMode.YByDistance;
            currentProperty.Color = accessory.Data.DefaultSafeColor;
            currentProperty.Delay = timelineControl;
            currentProperty.DestoryAt = 2250;
            timelineControl += 2250;

            accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            // ----- -----

            // ----- Be covered in the opposite group -----

            for (int i = myRoundToTakeHits + 1; i <= 4; ++i)
            {

                currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase5_Inward_Guidance_Of_Polarizing_Strikes_In_The_Opposite_Group";
                currentProperty.Scale = new(2);
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = inTheLeftGroup ?
                    RotatePoint(phase5_positionToBeCoveredOnTheRight_asAConstant, new Vector3(100, 0, 100), currentRotation) :
                    RotatePoint(phase5_positionToBeCoveredOnTheLeft_asAConstant, new Vector3(100, 0, 100), currentRotation);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.Delay = timelineControl;
                currentProperty.DestoryAt = 2450;
                timelineControl += 2450;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

                currentProperty = accessory.Data.GetDefaultDrawProperties();

                currentProperty.Name = "Phase5_Outward_Guidance_Of_Polarizing_Strikes_In_The_Opposite_Group";
                currentProperty.Scale = new(2);
                currentProperty.Owner = accessory.Data.Me;
                currentProperty.TargetPosition = inTheLeftGroup ?
                    RotatePoint(phase5_positionToStandbyOnTheRight_asAConstant, new Vector3(100, 0, 100), currentRotation) :
                    RotatePoint(phase5_positionToStandbyOnTheLeft_asAConstant, new Vector3(100, 0, 100), currentRotation);
                currentProperty.ScaleMode |= ScaleMode.YByDistance;
                currentProperty.Color = accessory.Data.DefaultSafeColor;
                currentProperty.Delay = timelineControl;
                currentProperty.DestoryAt = 2250;
                timelineControl += 2250;

                accessory.Method.SendDraw(DrawModeEnum.Imgui, DrawTypeEnum.Displacement, currentProperty);

            }

            System.Threading.Thread.Sleep(timeToTakeHits);

            if (Enable_Text_Prompts)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    accessory.Method.TextInfo("æŒ¡æžªç„¶åŽæ¢ç»„", 1500);

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    accessory.Method.TextInfo("Take hits and swap the group", 1500);

                }

            }

            if (Enable_Vanilla_TTS || Enable_Daily_Routines_TTS)
            {

                if (Language_Of_Prompts == Languages_Of_Prompts.Simplified_Chinese_ç®€ä½“ä¸­æ–‡)
                {

                    accessory.TTS("æŒ¡æžªç„¶åŽæ¢ç»„", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

                if (Language_Of_Prompts == Languages_Of_Prompts.English_è‹±æ–‡)
                {

                    accessory.TTS("Take hits and swap the group", Enable_Vanilla_TTS, Enable_Daily_Routines_TTS);

                }

            }

        }

        private int phase5_getRoundToTakeHits(int currentIndex)
        {

            if (Phase5_Order_During_Polarizing_Strikes == Phase5_Orders_During_Polarizing_Strikes.Tanks_Melees_Ranges_Healers_å¦å…‹è¿‘æˆ˜è¿œç¨‹å¥¶å¦ˆ_èŽ«çµå–µä¸ŽMMW)
            {

                if (currentIndex == 0 || currentIndex == 1)
                {
                    // Tanks.

                    return 1;

                }

                if (currentIndex == 4 || currentIndex == 5)
                {
                    // Melees.

                    return 2;

                }

                if (currentIndex == 6 || currentIndex == 7)
                {
                    // Ranges.

                    return 3;

                }

                if (currentIndex == 2 || currentIndex == 3)
                {
                    // Healers.

                    return 4;

                }

            }

            if (Phase5_Order_During_Polarizing_Strikes == Phase5_Orders_During_Polarizing_Strikes.Tanks_Healers_Melees_Ranges_å¦å…‹å¥¶å¦ˆè¿‘æˆ˜è¿œç¨‹)
            {

                if (currentIndex == 0 || currentIndex == 1)
                {
                    // Tanks.

                    return 1;

                }

                if (currentIndex == 2 || currentIndex == 3)
                {
                    // Healers.

                    return 2;

                }

                if (currentIndex == 4 || currentIndex == 5)
                {
                    // Melees.

                    return 3;

                }

                if (currentIndex == 6 || currentIndex == 7)
                {
                    // Ranges.

                    return 4;

                }

            }

            return -1;
            // Just a placeholder and should never be reached.

        }

        public static Vector2? mathPoint(Blade b1, Blade b2)
        {
            //Calculate sine and cosine of direction
            float s1 = (float)Math.Sin(b1.Rotation);
            float c1 = (float)Math.Cos(b1.Rotation);
            float s2 = (float)Math.Sin(b2.Rotation);
            float c2 = (float)Math.Cos(b2.Rotation);

            //Start point
            float x1 = (float)b1.X;
            float y1 = (float)b1.Y;
            float x2 = (float)b2.X;
            float y2 = (float)b2.Y;

            //Calculate denominator
            float d = s1 * c2 - s2 * c1;

            //Check validity
            if (Math.Abs(d) < 1e-10)
            {
                return null; // parallel
            }

            //Calculate intersection point, thanks Alo
            float X = (x1 * s1 * c2 - x2 * s2 * c1 - (y2 - y1) * c1 * c2) / d;
            float Y = (y2 * c2 * s1 - y1 * c1 * s2 + (x2 - x1) * s1 * s2) / d;

            return new Vector2(X, Y);
        }

        public static Vector2? middlePoint(Vector2? P1, Vector2? P2)
        {
            if (P1.HasValue && P2.HasValue)
            {
                float midX = (P1.Value.X + P2.Value.X) / 2;
                float midY = (P1.Value.Y + P2.Value.Y) / 2;
                return new Vector2(midX, midY);
            }
            return null; //return null if either point is null
        }

        //Get the point closest to the midpoint
        public static onPoint FindClosestOnPoint(List<onPoint> points, Vector2? target)
        {
            onPoint closestPoint = null;
            float closestDistance = float.MaxValue;
            foreach (var point in points)
            {
                // Calculate distance
                float distance = Vector2.Distance(point.OnCoord, target.Value);
                //If current distance is less than known minimum distance, update
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = point;
                }
            }
            return closestPoint;//return the closest point
        }

        //Farthest sub-point from the reference point
        public static Vector2 FindFarthestPoint(onPoint point, Vector2? referencePoint)
        {
            //Store all coordinates
            Vector2[] coords = { point.Coord1, point.Coord2, point.Coord3, point.Coord4 };
            float maxDistance = float.MinValue;//Initialize max distance
            Vector2 farthestCoord = Vector2.Zero;//Initialize farthest coordinate
                                                 //Traverse all coordinates to find the farthest
            foreach (var coord in coords)
            {
                float distance = Vector2.Distance(coord, referencePoint.Value);//Calculate distance
                if (distance > maxDistance)//If current distance is greater than known max distance
                {
                    maxDistance = distance;//Update max distance
                    farthestCoord = coord;//Update farthest coordinate
                }
            }
            return farthestCoord;//Return the farthest coordinate
        }

        //Closest sub-point from the reference point
        public static Vector2 FindClosestPoint(onPoint point, Vector2? referencePoint)
        {
            //Store all coordinates
            Vector2[] coords = { point.Coord1, point.Coord2, point.Coord3, point.Coord4 };

            float minDistance = float.MaxValue;//Initialize min distance
            Vector2 closestCoord = Vector2.Zero;//Initialize closest coordinate

            // Traverse all coordinates to find the closest
            foreach (var coord in coords)
            {
                float distance = Vector2.Distance(coord, referencePoint.Value);//Calculate distance
                if (distance < minDistance)//If current distance is less than known min distance
                {
                    minDistance = distance;//Update min distance
                    closestCoord = coord;//Update closest coordinate
                }
            }
            return closestCoord;//Return the closest coordinate
        }

        public static Vector3 Vector3Fucker(Vector2? V)
        {
            Vector3 result = new Vector3();
            if (V.HasValue)
            {
                result.X = V.Value.X;
                result.Y = 0;
                result.Z = V.Value.Y;
            }
            return result;
        }

        [ScriptMethod(name: "P5FulgentBlade", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40306"], userControl: false)]
        public void PhaseRecord_P5FulgentBlade(Event @event, ScriptAccessory accessory)
        {
            Phase = "P5FulgentBlade";
            blades.Clear();
            P1P3Blades.Clear();
            BladeRoutes.Clear();
            bladeCount = 0;
            BladeRoutes = Enumerable.Repeat<Vector2?>(null, 7).ToList();
            resetPoints();//Initialize fulgent blade coordinates
        }

        [ScriptMethod(name: "DebugSwitch", eventType: EventTypeEnum.StartCasting, eventCondition: ["ActionId:40306"], userControl: false)]
        public void PhaseRecord_P5Debug(Event @event, ScriptAccessory accessory)
        {
            if(Enable_Developer_Mode)accessory.Method.SendChat($"/e KnightRider wishes you smooth fulgent blades~");
        }

        //Capture group
        [ScriptMethod(name: "FulgentBladeDataCapture", eventType: EventTypeEnum.ObjectEffect, eventCondition: ["Id1:1"], userControl: false)]
        public void FulgentBladeDataCapture(Event @event, ScriptAccessory accessory)
        {
            if (Phase == "P5FulgentBlade")//capture restricted area
            {
                lock (bladeLock)
                {
                    if (bladeCount < 7) //if less than 7 blades, continue capturing
                    {
                        var pos = JsonConvert.DeserializeObject<Vector3>(@event["SourcePosition"]);
                        //Store data
                        blades.Add(new Blade(
                            id: Convert.ToUInt32(@event["SourceId"], 16),
                            x: Convert.ToDouble(pos.X),
                            y: Convert.ToDouble(pos.Z),
                            rotation: Convert.ToDouble(@event["SourceRotation"])
                        ));
                        bladeCount++;
                        //accessory.Method.SendChat($"/e {bladeCount}");
                    }
                    if (blades.Count == 6) //if 6 blade data collected
                    {
                        ProcessBlades();//process three intersection points + change phase => P5FulgentBladeCalcComplete
                        //accessory.Method.SendChat($"/e Start running from point {OnPoint.Name}");
                    }
                }
            }
        }
        
        private void ProcessBlades()
        {
            //accessory.Method.SendChat($"/e Collection complete");
            var sortedBlades = blades.OrderBy(b => b.Id).ToList();//Sort list by OID
            //accessory.Method.SendChat($"/e Sorting complete");
            if (sortedBlades != null)
            {
                //accessory.Method.SendChat($"/e Sorting complete");
                //Store points 1 and 3
                P1P3Blades.Add(sortedBlades[0]);
                P1P3Blades.Add(sortedBlades[1]);
                P1P3Blades.Add(sortedBlades[4]);
                P1P3Blades.Add(sortedBlades[5]);
                //Calculate three intersection points
                Point1 = mathPoint(sortedBlades[0], sortedBlades[1]);//Calculate 1st intersection point
                Point2 = mathPoint(sortedBlades[2], sortedBlades[3]);//Calculate 2nd intersection point
                Point3 = mathPoint(sortedBlades[4], sortedBlades[5]);//Calculate 3rd intersection point
                MiddlePoint = middlePoint(Point1, Point3);//Calculate midpoint of 1 and 3
                OnPoint = FindClosestOnPoint(onPoints,MiddlePoint);//Calculate which onPoint to start running from
                //accessory.Method.SendChat($"/e Start running from point {OnPoint.Name}");
                Phase = "P5FulgentBladeCalcComplete";
            }
        }

        #endregion
        
        #region Common_Mathematical_Wheels

        public static float convertDegree(float degree) {
            
            return degree*float.Pi/180f;
            
        }
        
        private int ParsTargetIcon(string id)
        {
            firstTargetIcon ??= int.Parse(id, System.Globalization.NumberStyles.HexNumber);
            return int.Parse(id, System.Globalization.NumberStyles.HexNumber) - (int)firstTargetIcon;
        }
        private static bool ParseObjectId(string? idStr, out ulong id)
        {
            id = 0;
            if (string.IsNullOrEmpty(idStr)) return false;
            try
            {
                var idStr2 = idStr.Replace("0x", "");
                id = ulong.Parse(idStr2, System.Globalization.NumberStyles.HexNumber);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Round to the nearest
        /// </summary>
        /// <param name="point"></param>
        /// <param name="centre"></param>
        /// <returns></returns>
        private int PositionTo8Dir(Vector3 point, Vector3 centre)
        {
            // Dirs: N = 0, NE = 1, ..., NW = 7
            var r = Math.Round(4 - 4 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 8;
            return (int)r;

        }
        private int PositionTo6Dir(Vector3 point, Vector3 centre)
        {
            var r = Math.Round(3 - 3 * Math.Atan2(point.X - centre.X, point.Z - centre.Z) / Math.PI) % 6;
            return (int)r;

        }
        private Vector3 RotatePoint(Vector3 point, Vector3 centre, float radian)
        {

            Vector2 v2 = new(point.X - centre.X, point.Z - centre.Z);

            var rot = (MathF.PI - MathF.Atan2(v2.X, v2.Y) + radian);
            var lenth = v2.Length();
            return new(centre.X + MathF.Sin(rot) * lenth, centre.Y, centre.Z - MathF.Cos(rot) * lenth);
        }
        
        #endregion
        
    }

    #region Other_Common_Wheels

    public static class Extensions
    {
        public static void TTS(this ScriptAccessory accessory, string text, bool isTTS, bool isDRTTS)
        {
            if (isTTS && isDRTTS)
            {
                accessory.Method.TTS(text);
            }
            else
            {
                if (isDRTTS)
                {
                    accessory.Method.SendChat($"/pdr tts {text}");
                }
                else if (isTTS)
                {
                    accessory.Method.TTS(text);
                }
            }
        }
    }

    public static class IndexHelper
    {
        /// <summary>
        /// Input player dataId, get the corresponding position index
        /// </summary>
        /// <param name="pid">Player SourceId</param>
        /// <param name="accessory"></param>
        /// <returns>Position index corresponding to the player</returns>
        public static int GetPlayerIdIndex(this ScriptAccessory accessory, uint pid)
        {
            // Get player IDX
            return accessory.Data.PartyList.IndexOf(pid);
        }

        /// <summary>
        /// Get the position index corresponding to the main perspective player
        /// </summary>
        /// <param name="accessory"></param>
        /// <returns>Position index corresponding to the main perspective player</returns>
        public static int GetMyIndex(this ScriptAccessory accessory)
        {
            return accessory.Data.PartyList.IndexOf(accessory.Data.Me);
        }

        /// <summary>
        /// Input player dataId, get the corresponding position title, output characters only for text output
        /// </summary>
        /// <param name="pid">Player SourceId</param>
        /// <param name="accessory"></param>
        /// <returns>Position title corresponding to the player</returns>
        public static string GetPlayerJobById(this ScriptAccessory accessory, uint pid)
        {
            // Get player job abbreviation, useless, only for DEBUG output
            var idx = accessory.Data.PartyList.IndexOf(pid);
            var str = accessory.GetPlayerJobByIndex(idx);
            return str;
        }

        /// <summary>
        /// Input position index, get the corresponding position title, output characters only for text output
        /// </summary>
        /// <param name="idx">Position index</param>
        /// <param name="fourPeople">Whether it's a four-person dungeon</param>
        /// <param name="accessory"></param>
        /// <returns></returns>
        public static string GetPlayerJobByIndex(this ScriptAccessory accessory, int idx, bool fourPeople = false)
        {
            var str = idx switch
            {
                0 => "MT",
                1 => fourPeople ? "H1" : "ST",
                2 => fourPeople ? "D1" : "H1",
                3 => fourPeople ? "D2" : "H2",
                4 => "D1",
                5 => "D2",
                6 => "D3",
                7 => "D4",
                _ => "unknown"
            };
            return str;
        }
    }
    
    #endregion
    
}
