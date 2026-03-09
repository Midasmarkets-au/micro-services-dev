<template>
  <div v-if="type === 'inFields'">
    <h3 style="color: #666666; font-weight: 400; font-size: 14px">
      {{ date.format("HH:mm:ss") }}
    </h3>
    <div style="font-size: 12px; color: rgba(113, 113, 113, 1)">
      {{ date.format("YYYY-M-D") }}
    </div>
  </div>
  <div v-else-if="type === 'withoutSec'">
    {{ date.format("YYYY-M-D HH:mm") }}
  </div>
  <div v-else-if="type === 'GMTinFields'">
    <div style="color: #4d4d4d; font-weight: 400; font-size: 14px">
      {{ getExactTime() }}
    </div>
    <div style="font-size: 12px; color: rgba(113, 113, 113, 1)">
      {{ getExactDate() }}
    </div>
  </div>
  <div v-else-if="type === 'GMToneLiner'">
    <span>{{ getExactDateAndTime() }}</span>
  </div>
  <!--style="font-size: 12px; color: rgba(113, 113, 113, 1)"-->
  <div v-else-if="type === 'oneLiner'">
    {{ date.format("YYYY-M-D HH:mm:ss") }}
  </div>
  <div v-else-if="type === 'customCSS'">
    <span>{{ date.format("YYYY-M-D HH:mm:ss") }}</span>
  </div>
  <div v-else-if="type === 'customCSSv2'">
    <span>{{ date.format("YYYY-M-D") }}</span>
  </div>
  <div v-else-if="type === 'eventShop'">
    <span>{{ date.format("YYYY-M-D") }}</span>
  </div>
  <!-- use  -->
  <span v-else-if="type === 'exactTime'">{{ getExactDateAndTime() }} GMT</span>
  <span v-else-if="type === 'reportTime'">{{ getExactDateAndTime() }}</span>
  <span v-else-if="type === 'reportDate'">{{ getExactDate() }}</span>
  <span v-else>{{ getDateAndTimeFromISOString() }}</span>
</template>

<script setup lang="ts">
import moment from "moment";
import { useStore } from "@/store";
import { computed } from "vue";

const props = defineProps<{
  dateIsoString: string | null | undefined;
  format?: string;
  type?: string;
}>();

const store = useStore();

const date = computed(() => {
  // const regex = /\.\d+Z$/;
  // let str = props.dateIsoString;
  // console.log(str);
  // if (regex.test(str)) {
  //   str = str.slice(0, -5);
  //   console.log(str);
  // }
  // return moment(str).locale(store.state.AuthModule.user.language);
  // return moment(props.dateIsoString).locale(
  //   store.state.AuthModule.user.language
  // );
  if (store.state.AuthModule.config.utcEnabled) {
    return moment.parseZone(props.dateIsoString);
  } else {
    return moment(props.dateIsoString);
    //return moment.parseZone(props.dateIsoString);
  }
});

const getExactDateAndTime = () => {
  const date = moment.parseZone(props.dateIsoString);
  const formattedDate = date.format("YYYY-MM-DD HH:mm:ss");

  if (date.year() === moment().year()) {
    return date.format("MM-DD HH:mm:ss");
  }

  return formattedDate;
};

const getExactDate = () => {
  const date = moment.parseZone(props.dateIsoString);
  const formattedDate = date.format("YYYY-MM-DD");
  return formattedDate;
};

const getExactTime = () => {
  const date = moment.parseZone(props.dateIsoString);
  const formattedDate = date.format("HH:mm:ss");
  return formattedDate;
};

const getDateAndTimeFromISOString = () => {
  if (date.value.year() === 1970 || date.value.year() === 1969) {
    return "-";
  }

  // const now = moment().locale(useStore().state.AuthModule.user.language);
  // const threeDaysAgo = now.clone().subtract(5, "days");

  // if (date.isBetween(threeDaysAgo, now)) {
  //   return date.fromNow();
  // }

  // return date.locale(useStore().state.AuthModule.user.language).fromNow();
  if (props.format) {
    return date.value.format(props.format);
  }
  if (date.value.year() === moment().year()) {
    return date.value.format("MM-DD HH:mm:ss");
  }
  return date.value.format("YYYY-MM-DD HH:mm:ss");
};
</script>

<style scoped></style>
