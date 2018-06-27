using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoWorldUnity3D
{
    class Proto
    {
        internal const int SIZE_FIELD_SIZE = 4;
        internal const int MAX_PAYLOAD_LEN = 1 * 1024 * 1024;

        internal const int CLIENTID_LENGTH = 16;
        internal const int ENTITYID_LENGTH = 16;

        internal const UInt16 MT_INVALID = 0;
        // Server Messages
        internal const UInt16 MT_SET_GAME_ID = 1;
        internal const UInt16 MT_SET_GATE_ID = 2;
        internal const UInt16 MT_NOTIFY_CREATE_ENTITY = 3;
        internal const UInt16 MT_NOTIFY_DESTROY_ENTITY = 4;
        internal const UInt16 MT_DECLARE_SERVICE = 5;
        internal const UInt16 MT_UNDECLARE_SERVICE = 6;
        internal const UInt16 MT_CALL_ENTITY_METHOD = 7;
        internal const UInt16 MT_CREATE_ENTITY_ANYWHERE = 8;
        internal const UInt16 MT_LOAD_ENTITY_ANYWHERE = 9;
        internal const UInt16 MT_NOTIFY_CLIENT_CONNECTED = 10;
        internal const UInt16 MT_NOTIFY_CLIENT_DISCONNECTED = 11;
        internal const UInt16 MT_CALL_ENTITY_METHOD_FROM_CLIENT = 12;
        internal const UInt16 MT_SYNC_POSITION_YAW_FROM_CLIENT = 13;
        internal const UInt16 MT_NOTIFY_ALL_GAMES_CONNECTED = 14;
        internal const UInt16 MT_NOTIFY_GATE_DISCONNECTED = 15;
        internal const UInt16 MT_START_FREEZE_GAME = 16;
        internal const UInt16 MT_START_FREEZE_GAME_ACK = 17;
        // Message types for migrating
        internal const UInt16 MT_MIGRATE_REQUEST = 18;
        internal const UInt16 MT_REAL_MIGRATE = 19;

        internal const UInt16 MT_GATE_SERVICE_MSG_TYPE_START = 1000;
        internal const UInt16 MT_REDIRECT_TO_GATEPROXY_MSG_TYPE_START = 1001; // messages that should be redirected to client proxy
        internal const UInt16 MT_CREATE_ENTITY_ON_CLIENT = 1002;
        internal const UInt16 MT_DESTROY_ENTITY_ON_CLIENT = 1003;
        internal const UInt16 MT_NOTIFY_MAP_ATTR_CHANGE_ON_CLIENT = 1004;
        internal const UInt16 MT_NOTIFY_MAP_ATTR_DEL_ON_CLIENT = 1005;
        internal const UInt16 MT_NOTIFY_LIST_ATTR_CHANGE_ON_CLIENT = 1006;
        internal const UInt16 MT_NOTIFY_LIST_ATTR_POP_ON_CLIENT = 1007;
        internal const UInt16 MT_NOTIFY_LIST_ATTR_APPEND_ON_CLIENT = 1008;
        internal const UInt16 MT_CALL_ENTITY_METHOD_ON_CLIENT = 1009;
        internal const UInt16 MT_UPDATE_POSITION_ON_CLIENT = 1010;
        internal const UInt16 MT_UPDATE_YAW_ON_CLIENT = 1011;
        internal const UInt16 MT_SET_CLIENTPROXY_FILTER_PROP = 1012;
        internal const UInt16 MT_CLEAR_CLIENTPROXY_FILTER_PROPS = 1013;
        internal const UInt16 MT_NOTIFY_MAP_ATTR_CLEAR_ON_CLIENT = 1014;

        // add more ...

        internal const UInt16 MT_REDIRECT_TO_GATEPROXY_MSG_TYPE_STOP = 1500;

        internal const UInt16 MT_CALL_FILTERED_CLIENTS = 1501;
        internal const UInt16 MT_SYNC_POSITION_YAW_ON_CLIENTS = 1502;

        // add more ...

        internal const UInt16 MT_GATE_SERVICE_MSG_TYPE_STOP = 2000;
    }
}
