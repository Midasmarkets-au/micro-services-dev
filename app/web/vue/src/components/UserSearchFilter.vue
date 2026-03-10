<template>
  <!--  -->
  <el-autocomplete
    class="w-400px"
    v-model="state"
    :fetch-suggestions="querySearchAsync"
    :placeholder="''"
    @keyup.enter="emitsAllResults"
    @select="handleSelect"
    :highlight-first-item="!multipleSelection"
    :clearable="true"
  >
    <template #append>
      <el-button :icon="Search" @click="emitsAllResults" />
    </template>
    <template #default="{ item }">
      <div class="d-flex flex-column border-bottom border-bottom-1">
        <div class="row w-400px">
          <div class="col-4">
            <i class="fa-regular fa-user"></i>
            <span class="ms-2">{{ item.displayName }}</span>
          </div>

          <div class="col-8">
            <i class="fa-regular fa-envelope"></i>
            <span class="ms-2">{{ item.email }}</span>
          </div>

          <div class="col-4">
            <i class="fa-solid fa-phone"></i>
            <span class="ms-2">
              {{ item.phoneNumber }}
            </span>
          </div>
          <div class="col-8">
            <i class="fa-solid fa-hashtag"></i>
            <span class="ms-2">
              {{ item.accountNumber }}
            </span>
          </div>
        </div>
      </div>
    </template>
  </el-autocomplete>
</template>

<script lang="ts" setup>
import { computed, onMounted, ref, watch } from "vue";
import AccountService from "@/projects/tenant/modules/accounts/services/AccountService";
import { Search } from "@element-plus/icons-vue";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import UserService from "@/projects/tenant/modules/users/services/UserService";

/**
 * *******************Notice*******************
 *
 *  handle search pagination is complex,
 *  refer to AccountClients.vue for Yixuan's resolution
 *
 * ********************************************
 */

const state = ref("");

const props = withDefaults(
  defineProps<{
    searchTypes: string;
    defaultCriteria?: any;
    searchTrigger?: boolean;
    multipleSelection?: boolean;
    requireEmptySearch?: boolean;
  }>(),
  {
    multipleSelection: true,
    requireEmptySearch: false,
  }
);

/**
 *
 * Watch searchTrigger is necessary for search pagination
 * Parent component should pass a boolean value to trigger search
 *
 * when parent component needs trigger a search (like get the second page of search result)
 *  => searchTrigger.value = !searchTrigger.value;
 */
watch(
  () => props.searchTrigger,
  async () => {
    const res = await getSearchedDataBasedOnCurrentState();
    emits("getResultsIds", res);
  }
);

const emits = defineEmits<{
  (e: "getResultsIds", res: any): void;
}>();

const searchedData = ref<any>();

const getSearchedDataBasedOnCurrentState = async () => {
  const data = await searchHandler.value(
    state.value,
    props.defaultCriteria.page
  );
  searchedData.value = data;
  return {
    ids: data.hits.map((x) => x.id),
    criteria: {
      page: data.page,
      total: data.totalHits,
      size: data.hitsPerPage,
    },
    data: data.hits,
  };
};

const searchHandler = computed(
  () =>
    ({
      user: UserService.fuzzySearchUsers,
    }[props.searchTypes] ?? (() => MsgPrompt.error("Invalid search type")))
);

const cbResults = ref<any>([]);
const legacyState = ref("");

const querySearchAsync = async (
  queryString: string,
  callback: (arg: any) => void
) => {
  if (queryString === "") {
    callback([]);
    return;
  }
  if (queryString === legacyState.value) {
    callback(cbResults.value);
    return;
  }

  const data = await searchHandler.value(queryString);
  searchedData.value = data;

  const results = data.hits.map((x) => ({
    ...x,
    displayName: x.displayName ?? `${x.firstName} ${x.lastName}`,
  }));
  legacyState.value = queryString;
  cbResults.value = results;
  callback(results);
};

const emitsAllResults = async (event) => {
  if (props.requireEmptySearch) {
    const res = await getSearchedDataBasedOnCurrentState();
    emits("getResultsIds", res);
    return;
  }
  console.log("state.value", state.value);
  if (state.value === "") {
    emits("getResultsIds", {
      ids: [],
      criteria: { page: 1, total: 0, size: 10 },
    });
    return;
  }
  if (!props.multipleSelection) return;
  if (!searchedData.value) return;
  console.log("emitsAllResults");
  waitSearchedDataNull(() => {
    const res = {
      ids: searchedData.value.hits.map((x) => x.id),
      criteria: {
        page: searchedData.value.page,
        total: searchedData.value.totalHits,
        size: searchedData.value.hitsPerPage,
      },
      data: searchedData.value.hits,
    };

    searchedData.value = null;
    event.target.blur();
    emits("getResultsIds", res);
  });
};

const handleSelect = (item: any) => {
  const res = {
    ids: [item.id],
    criteria: { page: 1, total: 1, size: 10 },
    data: [item],
  };

  // configOutputResults(res);
  searchedData.value = null;
  state.value = "";
  emits("getResultsIds", res);
};

function waitSearchedDataNull(callback) {
  let intervalId = setInterval(() => {
    if (searchedData.value.hits) {
      clearInterval(intervalId);
      callback();
    }
  }, 100); // Check every 100 milliseconds
}

onMounted(() => {
  //
});
</script>

<style scoped></style>
