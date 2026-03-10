<template>
  <div class="d-md-flex gap-4">
    <div v-if="filterOptions?.includes('peroid')" class="peroid-css">
      <div class="d-flex align-items-center h-100">
        <label class="filter-label me-2">
          {{ $t("fields.period") }}
        </label>
        <el-date-picker
          class="w-250px h-100 rounded-2"
          type="daterange"
          v-model="period"
          :start-placeholder="$t('fields.startDate')"
          :end-placeholder="$t('fields.endDate')"
          @change="updatePeriod"
        />
      </div>
    </div>
    <div v-if="filterOptions?.includes('search')">
      <div v-if="!isMobile" class="input-group">
        <select
          class="form-select fs-7"
          id="searchTypeSelect"
          v-model="searchType"
        >
          <option
            v-for="(option, index) in searchTypes"
            :key="index"
            :value="option.value"
          >
            {{ option.text }}
          </option>
        </select>
        <input
          type="text"
          class="form-control"
          name="searchText"
          v-model="searchText"
          @keyup.enter="search"
        />
        <button
          class="btn btn-success"
          type="button"
          @click="search"
          :disabled="isSearching"
        >
          {{ $t("action.search") }}
        </button>
        <span class="cursor-pointer mt-4 ms-4 clear-border" @click="clear"
          >Clear
        </span>
      </div>
      <div v-if="isMobile">
        <div class="row">
          <div class="input-group">
            <select
              class="form-select mobile-input"
              id="searchTypeSelect"
              v-model="searchType"
            >
              <option
                v-for="(option, index) in searchTypes"
                :key="index"
                :value="option.value"
              >
                {{ option.text }}
              </option>
            </select>
            <input
              type="text"
              class="form-control mobile-input"
              name="searchText"
              v-model="searchText"
              @keyup.enter="search"
            />
          </div>
          <div class="d-flex mx-auto justify-content-between mt-4">
            <button
              class="btn btn-success mobile-input"
              type="button"
              @click="search"
              :disabled="isSearching"
            >
              {{ $t("action.search") }}
            </button>
            <span class="cursor-pointer mt-4 ms-4 clear-border" @click="clear"
              >Clear
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { defineProps, ref } from "vue";
import { isMobile } from "@/core/config/WindowConfig";

const period = ref([] as any);
const isFiltered = ref(false);
const isSearching = ref(true);

const props = defineProps({
  types: {
    type: Array,
    required: true,
  },
  type: {
    type: String,
    required: true,
  },

  filterOptions: {
    type: Array,
    required: true,
  },
});
const searchTypes = ref(props.types);
const searchType = ref(props.type);
const searchText = ref(props.text);
const searchIb = ref(props.ib);
const filterOptions = ref(props.filterOptions);

const emits = defineEmits<{
  (e: "search"): void;
  (e: "updatePeroid"): void;
}>();
const search = () => {
  emits("search", {
    type: searchType.value,
    text: searchText.value,
    Ib: searchIb.value,
  });
};

const updatePeriod = () => {
  let from = "";
  let to = "";

  if (period.value === null) {
    isFiltered.value = false;
  } else {
    from = period.value[0];
    let toDateObj = new Date(period.value[1]);
    to = setToEndOfDay(toDateObj);

    isFiltered.value = true;
  }
  emits("updatePeroid", { from, to });
};

const clear = () => {
  if (isSearching.value) return;
  searchText.value = "";
  searchIb.value = 0;
  search();
};

const searching = (status) => {
  isSearching.value = status;
};
function setToEndOfDay(dateObj) {
  dateObj.setHours(23);
  dateObj.setMinutes(59);
  dateObj.setSeconds(59);

  return dateObj;
}
defineExpose({
  searching,
  isFiltered,
});
</script>
<style scoped>
@media (max-width: 768px) {
  .peroid-css {
    margin-bottom: 14px;
  }

  .mobile-input {
    font-size: 13px !important;
    height: fit-content;
  }
}
</style>
