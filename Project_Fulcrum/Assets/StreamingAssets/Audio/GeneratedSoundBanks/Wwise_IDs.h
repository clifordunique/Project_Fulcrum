/////////////////////////////////////////////////////////////////////////////////////////////////////
//
// Audiokinetic Wwise generated include file. Do not edit.
//
/////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef __WWISE_IDS_H__
#define __WWISE_IDS_H__

#include <AK/SoundEngine/Common/AkTypes.h>

namespace AK
{
    namespace EVENTS
    {
        static const AkUniqueID AMBIENT_START = 1892414574U;
        static const AkUniqueID BREAKABLE_BREAK = 4052040634U;
        static const AkUniqueID ENERGYCHARGE = 2390935913U;
        static const AkUniqueID ENERGYPULSE = 1403055964U;
        static const AkUniqueID FOOTSTEP = 1866025847U;
        static const AkUniqueID IMPACT_CRATER = 1796283983U;
        static const AkUniqueID IMPACT_SLAM = 3485259485U;
        static const AkUniqueID ITEM_EQUIP = 2146777153U;
        static const AkUniqueID JUMP = 3833651337U;
        static const AkUniqueID LANDING = 2548270042U;
        static const AkUniqueID MENU_HIGHLIGHT = 29030485U;
        static const AkUniqueID MENU_PAUSE = 2170009975U;
        static const AkUniqueID MENU_RESUME = 2263363174U;
        static const AkUniqueID MENU_SELECT = 4203375351U;
        static const AkUniqueID PAIN = 1996139369U;
        static const AkUniqueID PLAYMUSIC = 417627684U;
        static const AkUniqueID PUNCHHIT = 3343194546U;
        static const AkUniqueID PUNCHSWING = 2180951687U;
        static const AkUniqueID RIVERLOOPSTART = 1718773345U;
        static const AkUniqueID SET_GENDER_FEMALE = 757371536U;
        static const AkUniqueID SET_GENDER_MALE = 1541588287U;
        static const AkUniqueID SLIDELOOPSTART = 1119192506U;
        static const AkUniqueID STRANDJUMP = 187194041U;
        static const AkUniqueID SUPERJUMP = 188599998U;
        static const AkUniqueID WINDLOOPSTART = 1721707775U;
    } // namespace EVENTS

    namespace SWITCHES
    {
        namespace AMBIENTZONE
        {
            static const AkUniqueID GROUP = 170877025U;

            namespace SWITCH
            {
                static const AkUniqueID CITY = 3888786832U;
                static const AkUniqueID RUINS = 417916826U;
                static const AkUniqueID RURAL = 569754219U;
            } // namespace SWITCH
        } // namespace AMBIENTZONE

        namespace GENDER
        {
            static const AkUniqueID GROUP = 1776943274U;

            namespace SWITCH
            {
                static const AkUniqueID FEMALE = 2204441813U;
                static const AkUniqueID MALE = 3111576190U;
            } // namespace SWITCH
        } // namespace GENDER

        namespace LIFESTATE
        {
            static const AkUniqueID GROUP = 761044930U;

            namespace SWITCH
            {
                static const AkUniqueID ALIVE = 655265632U;
                static const AkUniqueID DEAD = 2044049779U;
                static const AkUniqueID STUNNED = 124361234U;
            } // namespace SWITCH
        } // namespace LIFESTATE

        namespace MOVETYPE
        {
            static const AkUniqueID GROUP = 4160739938U;

            namespace SWITCH
            {
                static const AkUniqueID IDLE = 1874288895U;
                static const AkUniqueID RUNNING = 3863236874U;
                static const AkUniqueID SLIDING = 472853913U;
                static const AkUniqueID WALKING = 340271938U;
                static const AkUniqueID WALLSLIDING = 2190485205U;
            } // namespace SWITCH
        } // namespace MOVETYPE

        namespace SHOETYPE
        {
            static const AkUniqueID GROUP = 2413944604U;

            namespace SWITCH
            {
                static const AkUniqueID METAL = 2473969246U;
                static const AkUniqueID NORMAL = 1160234136U;
            } // namespace SWITCH
        } // namespace SHOETYPE

        namespace SURFACECONTACTS
        {
            static const AkUniqueID GROUP = 2055121345U;

            namespace SWITCH
            {
                static const AkUniqueID AIRBORNE = 1785231519U;
                static const AkUniqueID CEILING = 3504694206U;
                static const AkUniqueID GROUND = 2528658256U;
                static const AkUniqueID LEFTWALL = 4239551170U;
                static const AkUniqueID RIGHTWALL = 1022564501U;
            } // namespace SWITCH
        } // namespace SURFACECONTACTS

        namespace TERRAINTYPE
        {
            static const AkUniqueID GROUP = 25772016U;

            namespace SWITCH
            {
                static const AkUniqueID CONCRETE = 841620460U;
                static const AkUniqueID DIRT = 2195636714U;
                static const AkUniqueID GLASS = 2449969375U;
                static const AkUniqueID GRASS = 4248645337U;
                static const AkUniqueID GRAVEL = 2185786256U;
                static const AkUniqueID METAL = 2473969246U;
                static const AkUniqueID MUD = 712897245U;
                static const AkUniqueID SAND = 803837735U;
                static const AkUniqueID TILE = 2637588553U;
                static const AkUniqueID WATER = 2654748154U;
                static const AkUniqueID WOOD = 2058049674U;
            } // namespace SWITCH
        } // namespace TERRAINTYPE

    } // namespace SWITCHES

    namespace GAME_PARAMETERS
    {
        static const AkUniqueID ALIVE = 655265632U;
        static const AkUniqueID CONTACT_AIRBORNE = 1214482312U;
        static const AkUniqueID CONTACT_CEILING = 2615974827U;
        static const AkUniqueID CONTACT_GROUND = 1693494255U;
        static const AkUniqueID CONTACT_LEFTWALL = 512381289U;
        static const AkUniqueID CONTACT_RIGHTWALL = 3748983100U;
        static const AkUniqueID CONTACT_SURFACECLING = 589832086U;
        static const AkUniqueID ENERGYLEVEL = 2286036297U;
        static const AkUniqueID GFORCE_CONTINUOUS = 714279773U;
        static const AkUniqueID GFORCE_INSTANT = 2916501359U;
        static const AkUniqueID HEALTH = 3677180323U;
        static const AkUniqueID KNEELING = 2487913616U;
        static const AkUniqueID SHOETYPE_ID = 3806191500U;
        static const AkUniqueID SLIDING = 472853913U;
        static const AkUniqueID SPEED = 640949982U;
        static const AkUniqueID TIMEDILATION = 2881936400U;
        static const AkUniqueID VELOCITY_X = 164475293U;
        static const AkUniqueID VELOCITY_Y = 164475292U;
        static const AkUniqueID VOLUME_EFFECTS = 3194824364U;
        static const AkUniqueID VOLUME_MUSIC = 3891337659U;
        static const AkUniqueID WALLSLIDING = 2190485205U;
        static const AkUniqueID WINDFORCE = 3087318520U;
    } // namespace GAME_PARAMETERS

    namespace BANKS
    {
        static const AkUniqueID INIT = 1355168291U;
        static const AkUniqueID ENVIRONMENT = 1229948536U;
        static const AkUniqueID FIGHTERCHAR = 725599200U;
        static const AkUniqueID UI = 1551306167U;
    } // namespace BANKS

    namespace BUSSES
    {
        static const AkUniqueID MASTER_AUDIO_BUS = 3803692087U;
    } // namespace BUSSES

    namespace AUX_BUSSES
    {
        static const AkUniqueID LARGEROOM = 187046019U;
        static const AkUniqueID SMALLROOM = 2933838247U;
    } // namespace AUX_BUSSES

    namespace AUDIO_DEVICES
    {
        static const AkUniqueID NO_OUTPUT = 2317455096U;
        static const AkUniqueID SYSTEM = 3859886410U;
    } // namespace AUDIO_DEVICES

}// namespace AK

#endif // __WWISE_IDS_H__
