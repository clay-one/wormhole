﻿namespace Wormhole.Api.Model
{
    public class Constants
    {
        public const int UrlMaxLength = 200;

        public const string OutputMessageTagRegex = "^[\\w-_]*$";
        public const int OutputMessageTagMaxLength = 50;

        public const string OutputMessageCategoryRegex = "^[\\w-_]*$";
        public const int OutputMessageCategoryMaxLength = 50;

        public const string TenantIdRegex = "^[\\w-_]*$";
        public const int TenantIdMaxLength = 50;

        public const string KafkaTopicRegex = "^[\\w-_]*$";
        public const int KafkaTopicMaxLength = 50;

        public const string ExternalKeyRegex = "^[\\w-_]*$";
        public const int ExternalKeyMaxLength = 50;
    }
}