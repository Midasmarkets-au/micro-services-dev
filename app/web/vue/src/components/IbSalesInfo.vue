<template>
  <div class="d-flex align-items-center text-start">
    <div>
      <a
        href="#"
        class="fs-6 text-gray-800 text-hover-primary fw-bold"
        @click="onUserClicked"
        >{{ props.user.displayName }}
        <span v-if="props.code">( {{ props.code }} )</span>
      </a>
      <div v-if="props.uid" class="fs-7 text-muted fw-semobold mt-1">
        UID: {{ props.uid }}
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { computed, inject } from "vue";
import InjectKeys from "@/core/types/TenantGlobalInjectionKeys";

const props = withDefaults(
  defineProps<{
    side?: string;
    rounded?: boolean;
    size?: string;
    isAdmin?: boolean;
    code?: string;
    uid?: number;
    user?: any;
  }>(),
  {
    rounded: false,
  }
);

const displayName = computed(
  () => `${props.user.firstName} ${props.user.lastName}`
);

const openUserDetailSider = inject<(partyId: number) => any>(
  InjectKeys.OPEN_USER_DETAILS
);

const onUserClicked = () => {
  if (props.user.partyId) {
    openUserDetailSider?.(props.user.partyId);
  }
};

/*
{
  "firstName": "string",
  "lastName": "string",
  "priorName": "string",
  "birthdate": "string",
  "gender": "string",
  "citizen": "string",
  "ccc": "string",
  "phone": "string",
  "email": "string",
  "address": "string",
  "idFrom": "string",
  "idNumber": "string",
  "idOffice": "string",
  "idIssueDate": "string",
  "idExpiryDate": "string"
}


{
  "nativeName": "string",
  "firstName": "string",
  "lastName": "string",
  "language": "string",
  "timeZone": "string",
  "referCode": "string",
  "countryCode": "string",
  "currency": "string",
  "ccc": "string",
  "birthday": "2023-08-03",
  "gender": 0,
  "citizen": "string",
  "address": "string",
  "idType": 0,
  "idNumber": "string",
  "idIssuer": "string",
  "idIssuedOn": "2023-08-03",
  "idExpireOn": "2023-08-03"
}
 */
</script>
