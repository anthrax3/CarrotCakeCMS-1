﻿ALTER TABLE [dbo].[carrot_DataInfo]
    ADD CONSTRAINT [PK_carrot_DataInfo] PRIMARY KEY NONCLUSTERED ([DataInfoID] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF) ON [PRIMARY];

