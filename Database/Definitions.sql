CREATE TABLE IF NOT EXISTS botstats (
    id bigint GENERATED BY DEFAULT AS IDENTITY,
    time timestamp with time zone NOT NULL,
    messagessent bigint NOT NULL,
    messagessentself bigint NOT NULL,
    storedmessages bigint NOT NULL,
    storedmessagetotalsize bigint NOT NULL,
    commands bigint NOT NULL,
    usercount bigint NOT NULL,
    heapsize bigint NOT NULL,
    timetakentotal bigint NOT NULL,
    CONSTRAINT pk_botstats PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS bottimes (
    id bigint GENERATED BY DEFAULT AS IDENTITY,
    lastdailytasksupdate timestamp with time zone NOT NULL,
    laststatupdate timestamp with time zone NOT NULL,
    CONSTRAINT pk_bottimes PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS combinations (
    id bigint GENERATED BY DEFAULT AS IDENTITY,
    element1 VARCHAR(16) NOT NULL,
    element2 VARCHAR(16) NOT NULL,
    element3 VARCHAR(16) NULL,
    result VARCHAR(16) NOT NULL,
    timecreated timestamp with time zone NOT NULL,
    difficulty integer NOT NULL,
    CONSTRAINT pk_combinations PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS currentstats (
    planetid bigint GENERATED BY DEFAULT AS IDENTITY,
    newcoins integer NOT NULL,
    messagessent integer NOT NULL,
    messagesuserssent integer NOT NULL,
    totalcoins integer NOT NULL,
    totalmessagesuserssent integer NOT NULL,
    totalmessagessent integer NOT NULL,
    laststatupdate timestamp with time zone NOT NULL,
    CONSTRAINT pk_currentstats PRIMARY KEY (planetid)
);


CREATE TABLE IF NOT EXISTS elements (
    id bigint GENERATED BY DEFAULT AS IDENTITY,
    name VARCHAR(16) NOT NULL,
    found integer NOT NULL,
    finder_id bigint NOT NULL,
    time_created timestamp with time zone NOT NULL,
    CONSTRAINT pk_elements PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS helps (
    id integer GENERATED BY DEFAULT AS IDENTITY,
    message VARCHAR(256) NOT NULL,
    CONSTRAINT pk_helps PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS messages (
    id bigint GENERATED BY DEFAULT AS IDENTITY,
    authorid bigint NOT NULL,
    memberid bigint NOT NULL,
    channelid bigint NOT NULL,
    planetid bigint NOT NULL,
    planetindex integer NOT NULL,
    content TEXT NULL,
    timesent timestamp with time zone NOT NULL,
    embeddata text NULL,
    mentionsdata text NULL,
    hash bytea NOT NULL,
    isdeleted boolean NOT NULL,
    searchvector tsvector GENERATED ALWAYS AS (to_tsvector('english', coalesce(content, ''))) STORED,
    replytoid bigint NULL,
    CONSTRAINT pk_messages PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS planetinfos (
    planetid bigint GENERATED BY DEFAULT AS IDENTITY,
    messagesstored integer NOT NULL,
    modules integer[] NOT NULL,
    CONSTRAINT pk_planetinfos PRIMARY KEY (planetid)
);


CREATE TABLE IF NOT EXISTS stats (
    id bigint GENERATED BY DEFAULT AS IDENTITY,
    planetid bigint NOT NULL,
    newcoins integer NOT NULL,
    messagesuserssent integer NOT NULL,
    messagessent integer NOT NULL,
    totalcoins integer NOT NULL,
    totalmessagesuserssent integer NOT NULL,
    totalmessagessent integer NOT NULL,
    time timestamp with time zone NOT NULL,
    CONSTRAINT pk_stats PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS suggestions (
    id bigint GENERATED BY DEFAULT AS IDENTITY,
    element1 VARCHAR(16) NOT NULL,
    element2 VARCHAR(16) NOT NULL,
    element3 VARCHAR(16) NULL,
    result VARCHAR(16) NOT NULL,
    userid bigint NOT NULL,
    timesuggested timestamp with time zone NOT NULL,
    ayes integer NOT NULL,
    nays integer NOT NULL,
    CONSTRAINT pk_suggestions PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS suggestionvotes (
    id bigint GENERATED BY DEFAULT AS IDENTITY,
    userid bigint NOT NULL,
    suggestionid bigint NOT NULL,
    CONSTRAINT pk_suggestionvotes PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS userembedstates (
    memberid bigint GENERATED BY DEFAULT AS IDENTITY,
    stringdata TEXT NOT NULL,
    CONSTRAINT pk_userembedstates PRIMARY KEY (memberid)
);


CREATE TABLE IF NOT EXISTS userinvitems (
    id bigint GENERATED BY DEFAULT AS IDENTITY,
    userid bigint NOT NULL,
    element VARCHAR(16) NOT NULL,
    timefound timestamp with time zone NOT NULL,
    CONSTRAINT pk_userinvitems PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS users (
    id bigint GENERATED BY DEFAULT AS IDENTITY,
    userid bigint NOT NULL,
    planetid bigint NOT NULL,
    coins integer NOT NULL,
    pointsthisminute smallint NOT NULL,
    totalpoints integer NOT NULL,
    totalchars integer NOT NULL,
    messagexp numeric(30,2) NOT NULL,
    elementalxp numeric(30,2) NOT NULL,
    gamexp numeric(30,2) NOT NULL,
    messages integer NOT NULL,
    activeminutes integer NOT NULL,
    lasthourly timestamp with time zone NOT NULL,
    lastsentmessage timestamp with time zone NOT NULL,
    lastupdateddailytasks date NOT NULL,
    xp numeric(30,2) NOT NULL,
    CONSTRAINT pk_users PRIMARY KEY (id)
);


CREATE TABLE IF NOT EXISTS dailytasks (
    id bigint GENERATED BY DEFAULT AS IDENTITY,
    memberid bigint NOT NULL,
    reward smallint NOT NULL,
    tasktype integer NOT NULL,
    goal smallint NOT NULL,
    done smallint NOT NULL,
    CONSTRAINT pk_dailytasks PRIMARY KEY (id),
    CONSTRAINT fk_dailytasks_users_memberid FOREIGN KEY (memberid) REFERENCES users (id) ON DELETE CASCADE
);


CREATE TABLE IF NOT EXISTS userstats (
    id bigint GENERATED BY DEFAULT AS IDENTITY,
    memberid bigint NOT NULL,
    totalcoins integer NOT NULL,
    totalpoints integer NOT NULL,
    totalchars integer NOT NULL,
    totalactiveminutes integer NOT NULL,
    totalmessages integer NOT NULL,
    totalxp numeric(30,2) NOT NULL,
    date date NOT NULL,
    CONSTRAINT pk_userstats PRIMARY KEY (id),
    CONSTRAINT fk_userstats_users_memberid FOREIGN KEY (memberid) REFERENCES users (id) ON DELETE CASCADE
);


CREATE INDEX IF NOT EXISTS ix_combinations_element1 ON combinations (element1);


CREATE INDEX IF NOT EXISTS ix_combinations_element2 ON combinations (element2);


CREATE INDEX IF NOT EXISTS ix_combinations_element3 ON combinations (element3);


CREATE INDEX IF NOT EXISTS ix_combinations_result ON combinations (result);


CREATE INDEX IF NOT EXISTS ix_dailytasks_memberid ON dailytasks (memberid);


CREATE INDEX IF NOT EXISTS ix_elements_name ON elements (name);


CREATE INDEX IF NOT EXISTS ix_messages_authorid ON messages (authorid);


CREATE UNIQUE INDEX IF NOT EXISTS ix_messages_hash ON messages (hash);


CREATE INDEX IF NOT EXISTS ix_messages_memberid ON messages (memberid);


CREATE INDEX IF NOT EXISTS ix_messages_planetid ON messages (planetid);


CREATE INDEX IF NOT EXISTS ix_messages_planetindex ON messages (planetindex);


CREATE INDEX IF NOT EXISTS ix_messages_searchvector ON messages USING GIN (searchvector);


CREATE INDEX IF NOT EXISTS ix_messages_timesent ON messages (timesent);


CREATE INDEX IF NOT EXISTS ix_stats_planetid ON stats (planetid);


CREATE INDEX IF NOT EXISTS ix_stats_time ON stats (time);


CREATE INDEX IF NOT EXISTS ix_userinvitems_element ON userinvitems (element);


CREATE INDEX IF NOT EXISTS ix_userinvitems_userid ON userinvitems (userid);


CREATE INDEX IF NOT EXISTS ix_userstats_date ON userstats (date);


CREATE INDEX IF NOT EXISTS ix_userstats_memberid ON userstats (memberid);


