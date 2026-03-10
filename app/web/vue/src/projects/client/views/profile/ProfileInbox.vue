<template>
  <div>
    <div>
      <CenterMenu activeMenuItem="inbox" />
    </div>
    <div class="banner" ref="announcementBannerRef" v-if="false">
      <div
        class="announcement-item"
        v-for="(announcement, index) in announcementList"
        :key="index"
      >
        {{ announcement }}
      </div>
    </div>

    <div v-if="isMobile" style="margin-top: 6.25rem">
      <div class="">
        <div class="account-tabs">
          <button
            class="account-tab"
            :class="{ active: !selectedTab }"
            @click="changeTab()"
          >
            {{ $t("action.all") }}
          </button>

          <button
            class="account-tab"
            :class="{ active: selectedTab === MessageTypes.Deposit }"
            @click="changeTab(MessageTypes.Deposit)"
          >
            {{ $t("action.deposit") }}
          </button>

          <button
            class="account-tab"
            :class="{ active: selectedTab === MessageTypes.Withdrawal }"
            @click="changeTab(MessageTypes.Withdrawal)"
          >
            {{ $t("action.withdraw") }}
          </button>

          <button
            class="account-tab"
            :class="{ active: selectedTab === MessageTypes.Transfer }"
            @click="changeTab(MessageTypes.Transfer)"
          >
            {{ $t("action.transfer") }}
          </button>

          <button
            class="account-tab"
            :class="{ active: selectedTab === MessageTypes.Payment }"
            @click="changeTab(MessageTypes.Payment)"
          >
            {{ $t("action.payment") }}
          </button>

          <button
            class="account-tab"
            :class="{ active: selectedTab === MessageTypes.Welcome }"
            @click="changeTab(MessageTypes.Welcome)"
          >
            {{ $t("action.newClient") }}
          </button>

          <button
            class="account-tab"
            :class="{ active: selectedTab === MessageTypes.Notice }"
            @click="changeTab(MessageTypes.Notice)"
          >
            {{ $t("action.notice") }}
          </button>
        </div>
      </div>
      <div class="" style="">
        <div v-if="!selectedMessage" class="h-100 mt-5">
          <div class="message-list">
            <NoDataCentralBox v-if="messageList.length === 0" />
            <div v-else class="message-list-container">
              <div
                class="message-card"
                v-for="(item, index) in messageList"
                :class="{
                  'active-message-card': selectedMessage?.id === item.id,
                }"
                :key="index"
                @click="goToMessageDetail(item)"
              >
                <div class="message-card-info">
                  <h3>{{ item.title }}</h3>
                  <div class="message-card-date">
                    <TimeShow
                      :date-iso-string="item.createdOn"
                      format="MMM D, YYYY"
                    />
                  </div>
                  <div
                    class="message-type"
                    :style="{
                      backgroundColor: {
                        [MessageTypes.Deposit]: '#0084A2',
                        [MessageTypes.Withdrawal]: '#00A284',
                        [MessageTypes.Transfer]: '#0671E0',
                        [MessageTypes.Payment]: '#838CA8',
                        [MessageTypes.Welcome]: '#8F00D2',
                        [MessageTypes.Notice]: '#D700A7',
                        [MessageTypes.System]: '#838CA8',
                      }[item.type],
                    }"
                  >
                    {{
                      {
                        [MessageTypes.Deposit]: $t("action.deposit"),
                        [MessageTypes.Withdrawal]: $t("action.withdraw"),
                        [MessageTypes.Transfer]: $t("action.transfer"),
                        [MessageTypes.Payment]: $t("action.payment"),
                        [MessageTypes.Welcome]: $t(
                          "action.newCustomerOpenAccount"
                        ),
                        [MessageTypes.Notice]: $t("action.notice"),
                        [MessageTypes.System]: $t("action.system"),
                      }[item.type] ?? $t("action.all")
                    }}
                  </div>
                </div>

                <div class="message-card-action" v-if="false">
                  <inline-svg src="/images/icons/general/delete.svg" />
                  <inline-svg src="/images/icons/general/star.svg" />
                </div>
              </div>
            </div>
            <div class="message-footer">
              <TableFooter
                class="m-0"
                :criteria="criteria"
                @page-change="fetchData"
              />
            </div>
          </div>
        </div>

        <div v-if="selectedMessage" class="h-100 mt-5">
          <div class="message-details">
            <div
              class="pb-5 text-uppercase d-flex align-items-center gap-2 cursor-pointer"
              @click="goBack"
              style="color: #4196f0"
            >
              <inline-svg src="/images/icons/arrows/left002.svg" />
              <span>{{ $t("action.back") }} </span>
            </div>
            <div class="details-title">
              <h2>{{ selectedMessage.title }}</h2>
              <TimeShow
                :date-iso-string="selectedMessage.createdOn"
                format="MMM D, YYYY"
              />
            </div>
            <div class="detail-content" v-html="selectedMessage.content"></div>
          </div>
        </div>
      </div>

      <!--      <div v-if="selectedMessage !== null" class="message-details">-->
      <!--        <div class="details-title">-->
      <!--          <h2>{{ selectedMessage.title }}</h2>-->
      <!--          <TimeShow-->
      <!--            :date-iso-string="selectedMessage.createdOn"-->
      <!--            format="MMM D, YYYY"-->
      <!--          />-->
      <!--        </div>-->
      <!--        <div class="detail-content" v-html="selectedMessage.content"></div>-->
      <!--      </div>-->
    </div>

    <!--    Deskop View-->
    <div
      class="w-100 h-650px pt-5 d-flex justify-content-between"
      v-if="!isMobile"
    >
      <div class="sider-bar">
        <ul class="">
          <li :class="{ selected: !selectedTab }" @click="changeTab()">
            <span
              class="svg-icon svg-icon-7 me-3"
              style="transform: rotate(-90deg)"
            >
              <inline-svg
                class="svg-not-selected"
                src="/images/icons/arrows/down001.svg"
              />
              <inline-svg
                class="svg-selected"
                src="/images/icons/arrows/down002.svg"
              />
            </span>

            <span>{{ $t("action.all") }}</span>
          </li>
          <li
            :class="{ selected: selectedTab === MessageTypes.Deposit }"
            @click="changeTab(MessageTypes.Deposit)"
          >
            <span
              class="svg-icon svg-icon-7 me-3"
              style="transform: rotate(-90deg)"
            >
              <inline-svg
                class="svg-not-selected"
                src="/images/icons/arrows/down003.svg"
              />
              <inline-svg
                class="svg-selected"
                src="/images/icons/arrows/down002.svg"
              />
            </span>
            <span> {{ $t("action.deposit") }}</span>
          </li>
          <li
            :class="{ selected: selectedTab === MessageTypes.Withdrawal }"
            @click="changeTab(MessageTypes.Withdrawal)"
          >
            <span
              class="svg-icon svg-icon-7 me-3"
              style="transform: rotate(-90deg)"
            >
              <inline-svg
                class="svg-not-selected"
                src="/images/icons/arrows/down003.svg"
              />
              <inline-svg
                class="svg-selected"
                src="/images/icons/arrows/down002.svg"
              />
            </span>
            <span>{{ $t("action.withdraw") }}</span>
          </li>
          <li
            :class="{ selected: selectedTab === MessageTypes.Transfer }"
            @click="changeTab(MessageTypes.Transfer)"
          >
            <span
              class="svg-icon svg-icon-7 me-3"
              style="transform: rotate(-90deg)"
            >
              <inline-svg
                class="svg-not-selected"
                src="/images/icons/arrows/down003.svg"
              />
              <inline-svg
                class="svg-selected"
                src="/images/icons/arrows/down002.svg"
              />
            </span>
            <span>{{ $t("action.transfer") }}</span>
          </li>

          <li
            :class="{ selected: selectedTab === MessageTypes.Payment }"
            @click="changeTab(MessageTypes.Payment)"
          >
            <span
              class="svg-icon svg-icon-7 me-3"
              style="transform: rotate(-90deg)"
            >
              <inline-svg
                class="svg-not-selected"
                src="/images/icons/arrows/down003.svg"
              />
              <inline-svg
                class="svg-selected"
                src="/images/icons/arrows/down002.svg"
              />
            </span>
            <span>{{ $t("action.payment") }}</span>
          </li>

          <li
            :class="{ selected: selectedTab === MessageTypes.Welcome }"
            @click="changeTab(MessageTypes.Welcome)"
          >
            <span
              class="svg-icon svg-icon-7 me-3"
              style="transform: rotate(-90deg)"
            >
              <inline-svg
                class="svg-not-selected"
                src="/images/icons/arrows/down003.svg"
              />
              <inline-svg
                class="svg-selected"
                src="/images/icons/arrows/down002.svg"
              />
            </span>
            <span> {{ $t("action.newCustomerOpenAccount") }}</span>
          </li>
          <li
            :class="{ selected: selectedTab === MessageTypes.Notice }"
            @click="changeTab(MessageTypes.Notice)"
          >
            <span
              class="svg-icon svg-icon-7 me-3"
              style="transform: rotate(-90deg)"
            >
              <inline-svg
                class="svg-not-selected"
                src="/images/icons/arrows/down003.svg"
              />
              <inline-svg
                class="svg-selected"
                src="/images/icons/arrows/down002.svg"
              /> </span
            ><span> {{ $t("action.notice") }}</span>
          </li>
        </ul>
      </div>
      <div class="inbox-details">
        <div class="message-list">
          <NoDataCentralBox v-if="messageList.length === 0" />
          <div v-else class="message-list-container">
            <div
              class="message-card"
              v-for="(item, index) in messageList"
              :class="{ 'active-message-card': selectedMessage.id === item.id }"
              :key="index"
              @click="goToMessageDetail(item)"
            >
              <div class="message-card-info">
                <h3>{{ item.title }}</h3>
                <div class="message-card-date">
                  <TimeShow
                    :date-iso-string="item.createdOn"
                    format="MMM D, YYYY"
                  />
                </div>
                <div
                  class="message-type"
                  :style="{
                    backgroundColor: {
                      [MessageTypes.Deposit]: '#0084A2',
                      [MessageTypes.Withdrawal]: '#00A284',
                      [MessageTypes.Transfer]: '#0671E0',
                      [MessageTypes.Payment]: '#838CA8',
                      [MessageTypes.Welcome]: '#8F00D2',
                      [MessageTypes.Notice]: '#D700A7',
                      [MessageTypes.System]: '#838CA8',
                    }[item.type],
                  }"
                >
                  {{
                    {
                      [MessageTypes.Deposit]: $t("action.deposit"),
                      [MessageTypes.Withdrawal]: $t("action.withdraw"),
                      [MessageTypes.Transfer]: $t("action.transfer"),
                      [MessageTypes.Payment]: $t("action.payment"),
                      [MessageTypes.Welcome]: $t(
                        "action.newCustomerOpenAccount"
                      ),
                      [MessageTypes.Notice]: $t("action.notice"),
                      [MessageTypes.System]: $t("action.system"),
                    }[item.type] ?? $t("action.all")
                  }}
                </div>
              </div>

              <div class="message-card-action" v-if="false">
                <inline-svg src="/images/icons/general/delete.svg" />
                <inline-svg src="/images/icons/general/star.svg" />
              </div>
            </div>
          </div>
          <div class="message-footer">
            <TableFooter
              class="m-0"
              :criteria="criteria"
              @page-change="fetchData"
            />
          </div>
        </div>
        <div class="message-details">
          <template v-if="selectedMessage !== null"
            ><div class="details-title">
              <h2>{{ selectedMessage.title }}</h2>
              <TimeShow
                :date-iso-string="selectedMessage.createdOn"
                format="MMM D, YYYY"
              />
            </div>
            <div class="detail-content" v-html="selectedMessage.content"></div
          ></template>
          <NoDataCentralBox v-else />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { nextTick, onMounted, onUnmounted, ref } from "vue";
import { MessageTypes } from "@/core/types/MessageTypes";
import GlobalService from "@/projects/client/services/ClientGlobalService";
import MsgPrompt from "@/core/plugins/MsgPrompt";
import TimeShow from "@/components/TimeShow.vue";
import TableFooter from "@/components/TableFooter.vue";
import NoDataCentralBox from "@/components/NoDataCentralBox.vue";
import { isMobile } from "@/core/config/WindowConfig";
import CenterMenu from "./components/CenterMenu.vue";
const selectedTab = ref<MessageTypes>();
const selectedMessage = ref<any>(null);

const messageList = ref(Array<any>());
const announcementList = ref(Array<any>());
const announcementBannerRef = ref<HTMLDivElement>();

const changeTab = (tab?: MessageTypes) => {
  selectedTab.value = tab;
  criteria.value.type = tab;

  fetchData(1);
};

const isLoading = ref(true);
let index = 0;
let intervalId: number | null = null;
const startSlideshow = () => {
  intervalId = setInterval(() => {
    index++;
    if (index >= announcementList.value.length) {
      index = 0;
    }
    announcementList.value.push(announcementList.value.shift());
  }, 2000); // every 2 seconds
};

const criteria = ref<any>({
  page: 1,
  size: 8,
});

const fetchData = async (_page: number) => {
  criteria.value.page = _page;
  try {
    isLoading.value = true;
    const res = await GlobalService.queryMessageInbox(criteria.value);
    messageList.value = res.data;
    // messageList.value = [];

    selectedMessage.value =
      !isMobile.value && messageList.value.length > 0
        ? messageList.value[0]
        : null;

    criteria.value = res.criteria;
    // criteria.value = {
    //   page: 1,
    //   size: 8,
    // };
  } catch (error) {
    MsgPrompt.error(error);
  } finally {
    isLoading.value = false;
  }
};

const fetchAnnouncement = async () => {
  try {
    announcementList.value = await (() =>
      Promise.resolve([
        "Announcement Placement 1",
        "Announcement Placement 2",
        "Announcement Placement 3",
      ]))();
  } catch (error) {
    MsgPrompt.error(error);
  }
};

const goToMessageDetail = (item: any) => {
  selectedMessage.value = item;
  // console.log(item);
};

const goBack = async () => {
  selectedMessage.value = null;
  await nextTick();
};

onUnmounted(() => {
  if (intervalId) {
    clearInterval(intervalId);
  }
});

onMounted(() => {
  fetchData(1);
  fetchAnnouncement();
  startSlideshow();
});
</script>

<style lang="scss" scoped>
.sub-menu {
  width: 100%;
  white-space: nowrap;
}
.banner {
  position: relative;
  display: flex;
  justify-content: center;
  align-content: center;
  color: #fff;
  padding: 18px 0;
  z-index: 0;
  width: 100%;
  overflow: hidden;

  .announcement-item {
    display: flex;
    justify-content: center;
    min-width: 100%;
    transition: transform 0.3s ease-out;
  }
}

.banner::before {
  content: "";
  border-radius: 16px;
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: #fff;
  opacity: 20%;
  z-index: -1;
}

.sider-bar {
  width: 13%;
  height: 95%;
  border-radius: 16px;
  background-color: #fff;
  box-shadow: 6px 4px 24px 0 rgba(184, 189, 205, 0.2),
    -4px -6px 24px 0 rgba(184, 189, 205, 0.2);
  overflow: hidden;

  & ul {
    padding: 0;
    margin: 0;
    list-style: none;
    color: #b1b1b1;

    li {
      display: flex;
      align-items: center;
      height: 60px;
      padding-left: 20px;
      cursor: pointer;
      transition: background-color 0.2s ease;

      &:hover {
        background-color: #002957;
        color: #ffd400;

        .svg-selected {
          display: block;
        }

        .svg-not-selected {
          display: none;
        }
      }

      .svg-selected {
        display: none;
      }
    }

    .selected {
      background-color: #002957;
      color: #ffd400;

      .svg-selected {
        display: block;
      }

      .svg-not-selected {
        display: none;
      }
    }
  }
}

.inbox-details {
  width: 85%;
  height: 95%;
  border-radius: 16px;
  box-shadow: 6px 4px 24px 0 rgba(184, 189, 205, 0.2),
    -4px -6px 24px 0 rgba(184, 189, 205, 0.2);
  background-color: #fbfbfb;
  overflow: hidden;

  display: flex;
  justify-content: space-between;
}

.message-list {
  width: 25%;
  height: 100%;
  position: relative;
  background-color: #fff;

  @media (max-width: 768px) {
    width: 100%;
  }

  .message-list-container {
    width: 100%;
    height: 90%;
    overflow: auto;

    .active-message-card {
      background-color: #f5f7fa;
    }

    .message-card {
      h3 {
        margin: 0;
        font-size: 16px;
        color: #212121;
      }

      display: flex;
      justify-content: space-between;
      padding: 15px;
      border-bottom: 1px solid #e4e6ef;

      transition: background-color 0.2s ease;

      cursor: pointer;
      &:hover {
        background-color: #f5f7fa;
      }

      .message-card-date {
        font-size: 12px;
        font-weight: 500;
        margin: 5px 0;
      }

      .message-type {
        display: inline-block;
        padding: 4px 16px;
        border-radius: 16px;
        font-size: 12px;
        color: #fff;
      }

      .message-card-action {
        margin-top: 5px;
        gap: 5px;
        display: flex;
        align-items: flex-start;
      }
    }
  }

  .message-footer {
    width: 100%;
    height: 10%;
    background-color: #fff;
    bottom: 0;

    & > div {
      margin: 10px 0 !important;
    }
  }
}

.message-details {
  width: 72%;
  height: 100%;
  background-color: #f5f7fa;
  padding: 20px;

  @media (max-width: 768px) {
    width: 100%;
    background-color: #fff;
  }

  .details-title {
    color: #4d4d4d;
    h2 {
      font-size: 20px;
      font-weight: 500;
    }
    padding-bottom: 10px;
    border-bottom: 1px solid #e4e6ef;
  }

  .detail-content {
    padding-top: 20px;
  }
}

.account-tabs {
  width: 100%;
  //height: 3rem;
  display: flex;
  overflow-x: auto;

  .account-tab {
    white-space: nowrap;
    width: 100px;
  }
}
</style>
