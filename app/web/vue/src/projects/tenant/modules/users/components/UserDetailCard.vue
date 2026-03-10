<template>
  <InputsDetailSec :details="info" :title="$t('title.personal')" />
  <InputsDetailSec :details="financial" :title="$t('title.financial')" />
  <InputsDetailSec
    :details="document"
    :title="$t('title.documents')"
    :show="true"
  />
  <RegisterDetailSec
    :title="$t('title.register')"
    :userId="info.idNumber"
    :userName="`${info.firstName} ${info.lastName}`"
    :userStatus="info.ccc"
    :isAgentAccount="true"
    :statusNote="`N/A`"
    :show="true"
  />
</template>

<script setup lang="ts">
import InputsDetailSec from "./InputsDetailSec.vue";
import RegisterDetailSec from "./RegisterDetailSec.vue";
import {
  VerificationDocument,
  VerificationFinancial,
  VerificationInfo,
  VerificationStarted,
} from "../types/VerificationDetails";

defineProps<{
  document: VerificationDocument;
  financial: VerificationFinancial;
  info: VerificationInfo;
  started: VerificationStarted;
}>();
</script>
