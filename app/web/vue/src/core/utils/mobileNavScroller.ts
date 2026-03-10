export const moibleNavScroller = (nav: string, navItem: string) => {
  const scroller = document.querySelector(nav) as HTMLElement;
  const destination = scroller?.querySelector(navItem) as HTMLElement;
  if (scroller && destination) {
    const leftOffset =
      destination.offsetLeft -
      scroller.offsetWidth / 2 +
      destination.offsetWidth / 2;
    scroller.scrollLeft = leftOffset;
  }
};
