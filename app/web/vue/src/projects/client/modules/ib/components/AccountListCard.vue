<template>
  <div>
    <router-link
      :to="'/ib/customers/' + data.uid + currentTabRoute"
      class="d-flex align-items-center text-start my-3 p-2 rounded"
      :class="{ 'customer-activate-bg': selected }"
      v-if="type == 'card'"
    >
      <div class="d-flex justify-content-between w-100 ms-3">
        <UserInfo
          :url="'/ib/customers/' + data.uid + currentTabRoute"
          :avatar="props.data.userInfo.avatar"
          :title="`${
            props.data.userInfo.firstName + '' + props.data.userInfo.lastName
          }`"
          :sub="props.data.uid"
          :name="props.data.name"
          class="me-2"
          side="client"
        />
        <span
          class="bullet bullet-vertical h-40px"
          :class="colors[props.data.status]"
        ></span>

        <!-- <div class="btn btn-primary btn-sm">{{ currentTabRoute }}</div> -->
        <!-- <div class="btn btn-primary btn-sm">
          {{ route.fullPath.indexOf(props.data.uid) }}
        </div> -->
      </div>
      <!-- <button class="btn btn-primary">{{ props.data.uid }}</button> -->
    </router-link>
  </div>
  <tr v-if="type == 'table'">
    <td class="">
      <div class="d-flex align-items-center">
        <UserAvatar
          :avatar="props.data.userInfo.avatar"
          :name="props.data.name"
          size="40px"
          class="me-3"
          side="client"
          rounded
        />
        <span style="color: ">
          {{
            props.data.userInfo.firstName +
              " " +
              props.data.userInfo.lastName ===
            " "
              ? "None"
              : props.data.userInfo.firstName +
                " " +
                props.data.userInfo.lastName
          }}
        </span>
      </div>
    </td>
    <td class="text-start">{{ props.data.uid }}</td>
    <td class="text-start">{{ $t(`type.accountRole.${props.data.role}`) }}</td>
    <td class="text-start">{{ props.data.group || "***" }}</td>
    <td class="text-start">{{ $t(`type.account.${props.data.type}`) }}</td>
    <td class="text-start">
      <TimeShow :date-iso-string="props.data.createdOn" />
    </td>
    <td class="text-start">
      <BalanceShow
        :balance="props.data.tradeAccount?.balanceInCents"
        :currency-id="props.data.tradeAccount?.currencyId"
      />
    </td>
    <td class="text-start">
      <router-link
        :to="'/ib/customers/' + data.uid"
        class="btn btn-info btn-sm"
      >
        {{ $t("action.details") }}
      </router-link>
    </td>
  </tr>
  <!-- <div class="btn btn-primary">{{ props.data }}</div> -->
</template>

<script lang="ts" setup>
import TimeShow from "@/components/TimeShow.vue";
import { ref, computed } from "vue";

import { useRoute } from "vue-router";
import BalanceShow from "@/components/BalanceShow.vue";
import UserInfo from "@/components/UserInfo.vue";
import UserAvatar from "@/components/UserAvatar.vue";

const route = useRoute();

const props = defineProps<{
  data: any;
  type: string;
}>();
// console.log(props.data);
const currentTabRoute = computed(() => {
  const pathSegments = route.fullPath.split("/");
  const lastWord = pathSegments.pop();
  return "/" + (lastWord && isNaN(parseInt(lastWord)) ? lastWord : "");
});

const colors = ref([
  "bg-success",
  "bg-warning",
  "bg-danger",
  "bg-primary",
  "bg-info",
]);

const selected = computed(() => props.data.uid == route.params.accountId);
// const ff = () => {
//   console.log(props.data);
//   console.log(selected.value);
// };
</script>
